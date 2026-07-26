using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.Timing;
using GameHub.Catalog;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Multiplayer
{
    /// <summary>
    /// Implements lightweight matchmaking for multiplayer games.
    /// </summary>
    public class MatchmakingService : GameHubDomainServiceBase, IMatchmakingService
    {
        public IAbpSession AbpSession { get; set; }

        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<MatchState, Guid> _matchRepository;
        private readonly IRepository<MatchParticipant, Guid> _participantRepository;

        public MatchmakingService(
            IRepository<Game, Guid> gameRepository,
            IRepository<MatchState, Guid> matchRepository,
            IRepository<MatchParticipant, Guid> participantRepository)
        {
            AbpSession = NullAbpSession.Instance;
            _gameRepository = gameRepository;
            _matchRepository = matchRepository;
            _participantRepository = participantRepository;
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
                var activeCount = await _participantRepository.CountAsync(p => p.MatchId == match.Id && p.IsActive);
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

        public async Task<MatchParticipant> JoinMatchAsync(Guid matchId, long? userId, string anonymousIdHash, string connectionId)
        {
            var match = await _matchRepository.GetAll()
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
            {
                throw new InvalidOperationException("Match not found.");
            }

            var activeCount = await _participantRepository.CountAsync(p => p.MatchId == matchId && p.IsActive);
            if (match.Status != MatchStatus.Waiting || activeCount >= match.MaxPlayers)
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
                JoinedAt = Clock.Now
            };

            await _participantRepository.InsertAsync(participant);

            if (activeCount + 1 >= match.MaxPlayers)
            {
                match.Start();
            }

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

            var match = await _matchRepository.GetAsync(matchId);
            if (match.Status == MatchStatus.InProgress && !await HasActiveParticipantsAsync(matchId))
            {
                match.End();
            }

            return true;
        }

        public async Task UpdateMatchStateAsync(Guid matchId, string payloadJson)
        {
            var match = await _matchRepository.GetAsync(matchId);
            match.PayloadJson = payloadJson;
        }

        public async Task EndMatchAsync(Guid matchId)
        {
            var match = await _matchRepository.GetAsync(matchId);
            match.End();
        }

        private async Task<bool> HasActiveParticipantsAsync(Guid matchId)
        {
            return await _participantRepository.GetAll()
                .AnyAsync(p => p.MatchId == matchId && p.IsActive);
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
