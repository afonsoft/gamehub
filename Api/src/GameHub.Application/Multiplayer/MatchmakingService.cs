using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.Timing;
using GameHub.Catalog;
using GameHub.Monitoring;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Multiplayer
{
    /// <summary>
    /// Implements lightweight matchmaking for multiplayer games.
    /// </summary>
    public class MatchmakingService : GameHubDomainServiceBase, IMatchmakingService
    {
        public const int GracePeriodSeconds = 30;
        public const int MaxSpectatorsPerMatch = 10;
        public const int MaxPayloadBytes = 64 * 1024;
        public IAbpSession AbpSession { get; set; }

        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<MatchState, Guid> _matchRepository;
        private readonly IRepository<MatchParticipant, Guid> _participantRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public MatchmakingService(
            IRepository<Game, Guid> gameRepository,
            IRepository<MatchState, Guid> matchRepository,
            IRepository<MatchParticipant, Guid> participantRepository,
            IUnitOfWorkManager unitOfWorkManager)
        {
            AbpSession = NullAbpSession.Instance;
            _gameRepository = gameRepository;
            _matchRepository = matchRepository;
            _participantRepository = participantRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<MatchState> CreateMatchAsync(Guid gameId, string mode, int? maxPlayers = null)
        {
            var game = await _gameRepository.GetAsync(gameId);
            if (!game.SupportsMultiplayer)
            {
                throw new InvalidOperationException("Game does not support multiplayer.");
            }

            var max = maxPlayers ?? Math.Max(2, game.MaxPlayersPerMatch);
            var roomCode = await GenerateUniqueRoomCodeAsync();
            var match = new MatchState(Guid.NewGuid(), gameId, roomCode, mode, max)
            {
                TenantId = AbpSession.TenantId,
                ExpiresAt = Clock.Now.AddHours(4)
            };

            await _matchRepository.InsertAsync(match);
            await CurrentUnitOfWork.SaveChangesAsync();
            GameHubMetrics.MatchesCreated.Add(1);
            return match;
        }

        public async Task<MatchState> FindOrCreateMatchAsync(Guid gameId, string mode, int? maxPlayers = null)
        {
            var game = await _gameRepository.GetAsync(gameId);
            if (!game.SupportsMultiplayer)
            {
                throw new InvalidOperationException("Game does not support multiplayer.");
            }

            var max = maxPlayers ?? Math.Max(2, game.MaxPlayersPerMatch);
            var waitingMatches = await _matchRepository.GetAll()
                .Where(m => m.GameId == gameId
                            && m.Mode == mode
                            && m.Status == MatchStatus.Waiting
                            && m.ExpiresAt > Clock.Now)
                .OrderBy(m => m.CreationTime)
                .ToListAsync();

            foreach (var match in waitingMatches)
            {
                var activeCount = await CountActiveParticipantsAsync(match.Id, false);
                if (activeCount < match.MaxPlayers)
                {
                    return match;
                }
            }

            return await CreateMatchAsync(gameId, mode, max);
        }

        public async Task<MatchState> GetMatchAsync(Guid matchId)
        {
            return await _matchRepository.GetAll()
                .Include(m => m.Participants)
                .FirstOrDefaultAsync(m => m.Id == matchId)
                ?? throw new InvalidOperationException("Match not found.");
        }

        public async Task<MatchState> GetMatchByRoomCodeAsync(string roomCode)
        {
            return await _matchRepository.GetAll()
                .Include(m => m.Participants)
                .Where(m => m.RoomCode == roomCode && m.Status != MatchStatus.Ended && m.ExpiresAt > Clock.Now)
                .OrderByDescending(m => m.CreationTime)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Match not found or expired.");
        }

        public async Task<MatchParticipant> JoinMatchAsync(Guid matchId, long? userId, string anonymousIdHash, string connectionId, bool isSpectator = false)
        {
            var match = await _matchRepository.GetAll()
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
            {
                throw new InvalidOperationException("Match not found.");
            }

            var activeCount = await CountActiveParticipantsAsync(matchId, false);
            var spectatorCount = await CountActiveParticipantsAsync(matchId, true);
            if (isSpectator && spectatorCount >= MaxSpectatorsPerMatch)
            {
                throw new InvalidOperationException("Spectator limit reached.");
            }

            if (!isSpectator && (match.Status != MatchStatus.Waiting || activeCount >= match.MaxPlayers))
            {
                throw new InvalidOperationException("Match is full or already started.");
            }

            var participant = new MatchParticipant
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                MatchId = matchId,
                UserId = userId,
                AnonymousIdHash = anonymousIdHash,
                ConnectionId = connectionId,
                IsActive = true,
                IsSpectator = isSpectator,
                JoinedAt = Clock.Now
            };

            await _participantRepository.InsertAsync(participant);

            if (!isSpectator && activeCount + 1 >= match.MaxPlayers)
            {
                match.Start();
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            GameHubMetrics.PlayersConnected.Add(1);
            return participant;
        }

        public async Task<MatchParticipant> ReactivateParticipantAsync(Guid matchId, long? userId, string anonymousIdHash, string connectionId)
        {
            var participant = await _participantRepository.GetAll()
                .Where(p => p.MatchId == matchId
                            && p.IsActive
                            && p.DisconnectedAt.HasValue
                            && p.GracePeriodEndsAt > Clock.Now
                            && ((userId.HasValue && p.UserId == userId) || (!userId.HasValue && p.AnonymousIdHash == anonymousIdHash)))
                .OrderByDescending(p => p.DisconnectedAt)
                .FirstOrDefaultAsync();

            if (participant == null)
            {
                return null;
            }

            participant.ConnectionId = connectionId;
            participant.DisconnectedAt = null;
            participant.GracePeriodEndsAt = null;
            await CurrentUnitOfWork.SaveChangesAsync();
            return participant;
        }

        public async Task<bool> LeaveMatchAsync(Guid matchId, string connectionId)
        {
            var participant = await _participantRepository.FirstOrDefaultAsync(p => p.MatchId == matchId && p.ConnectionId == connectionId && p.IsActive);
            if (participant == null)
            {
                return false;
            }

            participant.IsActive = false;
            participant.LeftAt = Clock.Now;
            participant.DisconnectedAt = null;
            participant.GracePeriodEndsAt = null;

            return true;
        }

        public async Task<bool> DisconnectAsync(string connectionId)
        {
            if (CurrentUnitOfWork == null)
            {
                using (var uow = _unitOfWorkManager.Begin())
                {
                    var result = await DisconnectAsync(connectionId);
                    await uow.CompleteAsync();
                    return result;
                }
            }

            var participant = await _participantRepository.FirstOrDefaultAsync(
                p => p.ConnectionId == connectionId && p.IsActive && !p.DisconnectedAt.HasValue);
            if (participant == null)
            {
                return false;
            }

            participant.DisconnectedAt = Clock.Now;
            participant.GracePeriodEndsAt = Clock.Now.AddSeconds(GracePeriodSeconds);
            if (CurrentUnitOfWork != null)
            {
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            return true;
        }

        public async Task UpdateMatchStateAsync(Guid matchId, string payloadJson, string connectionId = null)
        {
            ValidatePayload(payloadJson);
            var match = await _matchRepository.GetAsync(matchId);

            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                var participant = await _participantRepository.FirstOrDefaultAsync(
                    p => p.MatchId == matchId && p.ConnectionId == connectionId && p.IsActive);
                if (participant == null)
                {
                    throw new UnauthorizedAccessException("The connection is not a participant in this match.");
                }

                if (participant.IsSpectator)
                {
                    throw new UnauthorizedAccessException("Spectators cannot update match state.");
                }
            }

            match.PayloadJson = payloadJson;
            GameHubMetrics.MessagesSent.Add(1);
        }

        public async Task EndMatchAsync(Guid matchId)
        {
            var match = await _matchRepository.GetAsync(matchId);
            match.End();
        }

        public async Task<int> CleanupExpiredMatchesAsync()
        {
            var matches = await _matchRepository.GetAll()
                .Where(m => m.Status != MatchStatus.Ended && m.ExpiresAt <= Clock.Now)
                .ToListAsync();
            foreach (var match in matches)
            {
                match.End();
            }

            return matches.Count;
        }

        public async Task<int> CleanupDisconnectedParticipantsAsync()
        {
            var participants = await _participantRepository.GetAll()
                .Where(p => p.IsActive && p.GracePeriodEndsAt.HasValue && p.GracePeriodEndsAt <= Clock.Now)
                .ToListAsync();
            foreach (var participant in participants)
            {
                participant.IsActive = false;
                participant.LeftAt = Clock.Now;
                participant.DisconnectedAt = null;
                participant.GracePeriodEndsAt = null;
            }

            return participants.Count;
        }

        private async Task<int> CountActiveParticipantsAsync(Guid matchId, bool spectators)
        {
            return await _participantRepository.GetAll()
                .CountAsync(p => p.MatchId == matchId
                                 && p.IsActive
                                 && p.IsSpectator == spectators
                                 && (!p.GracePeriodEndsAt.HasValue || p.GracePeriodEndsAt > Clock.Now));
        }

        private static void ValidatePayload(string payloadJson)
        {
            var bytes = Encoding.UTF8.GetByteCount(payloadJson ?? string.Empty);
            if (bytes > MaxPayloadBytes)
            {
                throw new InvalidOperationException($"Match payload exceeds maximum size of {MaxPayloadBytes} bytes.");
            }

            if (string.IsNullOrWhiteSpace(payloadJson))
            {
                return;
            }

            try
            {
                using var document = JsonDocument.Parse(payloadJson);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Match payload must be valid JSON.", ex);
            }
        }

        private async Task<string> GenerateUniqueRoomCodeAsync()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            for (int attempt = 0; attempt < 10; attempt++)
            {
                var code = new char[6];
                for (int i = 0; i < code.Length; i++)
                {
                    code[i] = chars[random.Next(chars.Length)];
                }

                var roomCode = new string(code);
                var exists = await _matchRepository.GetAll().AnyAsync(m => m.RoomCode == roomCode);
                if (!exists)
                {
                    return roomCode;
                }
            }

            return Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        }
    }
}
