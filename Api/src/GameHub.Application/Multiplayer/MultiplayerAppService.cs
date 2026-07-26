using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using GameHub.Multiplayer.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Multiplayer
{
    /// <summary>
    /// Application service for multiplayer match management.
    /// </summary>
    [AbpAllowAnonymous]
    public class MultiplayerAppService : GameHubAppServiceBase, IMultiplayerAppService
    {
        private readonly IMatchmakingService _matchmakingService;
        private readonly IRepository<MatchState, Guid> _matchRepository;
        private readonly IRepository<RankedSeason, Guid> _seasonRepository;
        private readonly IRepository<PlayerRating, Guid> _ratingRepository;
        private readonly IRepository<RankedQueueEntry, Guid> _queueRepository;
        private readonly IRepository<MatchHistory, Guid> _historyRepository;
        private readonly IRepository<ReplayMetadata, Guid> _replayRepository;
        private readonly IRepository<MultiplayerSecurityEvent, Guid> _securityEventRepository;

        public MultiplayerAppService(
            IMatchmakingService matchmakingService,
            IRepository<MatchState, Guid> matchRepository,
            IRepository<RankedSeason, Guid> seasonRepository,
            IRepository<PlayerRating, Guid> ratingRepository,
            IRepository<RankedQueueEntry, Guid> queueRepository,
            IRepository<MatchHistory, Guid> historyRepository,
            IRepository<ReplayMetadata, Guid> replayRepository,
            IRepository<MultiplayerSecurityEvent, Guid> securityEventRepository)
        {
            _matchmakingService = matchmakingService;
            _matchRepository = matchRepository;
            _seasonRepository = seasonRepository;
            _ratingRepository = ratingRepository;
            _queueRepository = queueRepository;
            _historyRepository = historyRepository;
            _replayRepository = replayRepository;
            _securityEventRepository = securityEventRepository;
        }

        public async Task<MatchDto> CreateMatchAsync(CreateMatchInput input)
        {
            var match = await _matchmakingService.CreateMatchAsync(input.GameId, input.Mode, input.MaxPlayers);
            return await GetMatchAsync(match.Id);
        }

        public async Task<MatchDto> CreateOrJoinMatchAsync(CreateMatchInput input)
        {
            var match = await _matchmakingService.FindOrCreateMatchAsync(input.GameId, input.Mode, input.MaxPlayers);
            return await GetMatchAsync(match.Id);
        }

        public async Task<MatchDto> JoinMatchAsync(JoinMatchInput input)
        {
            var userId = AbpSession.UserId;
            var participant = await _matchmakingService.ReactivateParticipantAsync(
                input.MatchId, userId, input.AnonymousIdHash, input.ConnectionId);
            if (participant == null)
            {
                await _matchmakingService.JoinMatchAsync(
                    input.MatchId, userId, input.AnonymousIdHash, input.ConnectionId, input.IsSpectator);
            }
            return await GetMatchAsync(input.MatchId);
        }

        public async Task<MatchDto> JoinMatchByRoomCodeAsync(JoinMatchByRoomCodeInput input)
        {
            var match = await _matchRepository.GetAll()
                .Include(m => m.Participants)
                .Where(m => m.RoomCode == input.RoomCode && m.Status != MatchStatus.Ended && m.ExpiresAt > Clock.Now)
                .OrderByDescending(m => m.CreationTime)
                .FirstOrDefaultAsync();

            if (match == null)
            {
                throw new InvalidOperationException("Match not found or expired.");
            }

            var userId = AbpSession.UserId;
            var participant = await _matchmakingService.ReactivateParticipantAsync(
                match.Id, userId, input.AnonymousIdHash, input.ConnectionId);
            if (participant == null)
            {
                await _matchmakingService.JoinMatchAsync(
                    match.Id, userId, input.AnonymousIdHash, input.ConnectionId, input.IsSpectator);
            }
            return await GetMatchAsync(match.Id);
        }

        public Task<MatchDto> SpectateMatchAsync(Guid matchId, string anonymousIdHash = null, string connectionId = null)
        {
            return JoinMatchAsync(new JoinMatchInput
            {
                MatchId = matchId,
                AnonymousIdHash = anonymousIdHash,
                ConnectionId = connectionId ?? string.Empty,
                IsSpectator = true
            });
        }

        public async Task LeaveMatchAsync(LeaveMatchInput input)
        {
            await _matchmakingService.LeaveMatchAsync(input.MatchId, input.ConnectionId);
        }

        public async Task<MatchDto> GetMatchAsync(Guid matchId)
        {
            var match = await _matchRepository.GetAll()
                .Include(m => m.Participants)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
            {
                throw new InvalidOperationException("Match not found.");
            }

            return ObjectMapper.Map<MatchDto>(match);
        }

        public async Task UpdateMatchStateAsync(UpdateMatchStateInput input)
        {
            await _matchmakingService.UpdateMatchStateAsync(input.MatchId, input.PayloadJson, input.ConnectionId);
        }

        public async Task EndMatchAsync(Guid matchId)
        {
            await _matchmakingService.EndMatchAsync(matchId);
        }

        public async Task<List<MatchBrowserDto>> BrowseMatchesAsync(BrowseMatchesInput input)
        {
            var matches = await _matchmakingService.BrowseMatchesAsync(
                input.GameId, input.Mode, input.Region, input.MaxLatencyMs, input.IsRanked,
                input.SkipCount, input.MaxResultCount);
            return matches.Select(match => new MatchBrowserDto
            {
                MatchId = match.Id,
                GameId = match.GameId,
                RoomCode = match.RoomCode,
                Mode = match.Mode,
                Region = match.Region,
                Players = match.Participants.Count(participant => participant.IsActive && !participant.IsSpectator),
                Spectators = match.Participants.Count(participant => participant.IsActive && participant.IsSpectator),
                MaxPlayers = match.MaxPlayers,
                AverageLatencyMs = match.AverageLatencyMs,
                IsRanked = match.IsRanked,
                Status = match.Status,
                CreatedAt = match.CreationTime
            }).ToList();
        }

        public async Task<RankedQueueDto> EnqueueRankedAsync(EnqueueRankedInput input)
        {
            var userId = AbpSession.UserId ?? throw new Abp.Authorization.AbpAuthorizationException("Authentication required.");
            var season = await _seasonRepository.FirstOrDefaultAsync(s =>
                s.GameId == input.GameId && s.Mode == input.Mode && s.IsActive);
            if (season == null)
            {
                season = await _seasonRepository.InsertAsync(new RankedSeason
                {
                    Id = Guid.NewGuid(),
                    TenantId = AbpSession.TenantId,
                    GameId = input.GameId,
                    Mode = input.Mode,
                    Name = "Season 1",
                    StartsAt = Clock.Now,
                    IsActive = true
                });
            }

            var rating = await _ratingRepository.FirstOrDefaultAsync(r =>
                r.GameId == input.GameId && r.SeasonId == season.Id && r.Mode == input.Mode && r.UserId == userId);
            if (rating == null)
            {
                rating = await _ratingRepository.InsertAsync(new PlayerRating
                {
                    Id = Guid.NewGuid(),
                    TenantId = AbpSession.TenantId,
                    GameId = input.GameId,
                    SeasonId = season.Id,
                    UserId = userId,
                    Mode = input.Mode
                });
            }

            var current = await _queueRepository.FirstOrDefaultAsync(q =>
                q.UserId == userId && q.GameId == input.GameId && q.Mode == input.Mode && q.Status == RankedQueueStatus.Waiting);
            if (current != null)
            {
                return MapQueue(current);
            }

            var entry = await _queueRepository.InsertAsync(new RankedQueueEntry
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                SeasonId = season.Id,
                UserId = userId,
                Mode = input.Mode,
                Region = input.Region,
                RatingSnapshot = rating.Rating,
                EnqueuedAt = Clock.Now,
                Status = RankedQueueStatus.Waiting
            });

            var candidate = await _queueRepository.GetAll()
                .Where(queue => queue.Id != entry.Id
                                && queue.GameId == entry.GameId
                                && queue.SeasonId == entry.SeasonId
                                && queue.Mode == entry.Mode
                                && queue.Region == entry.Region
                                && queue.Status == RankedQueueStatus.Waiting
                                && Math.Abs(queue.RatingSnapshot - entry.RatingSnapshot) <= 200)
                .OrderBy(queue => queue.EnqueuedAt)
                .FirstOrDefaultAsync();
            if (candidate != null)
            {
                var match = await _matchmakingService.CreateMatchAsync(entry.GameId, entry.Mode, 2);
                match.IsRanked = true;
                match.RankedSeasonId = entry.SeasonId;
                match.Region = entry.Region;
                await _matchmakingService.JoinMatchAsync(match.Id, entry.UserId, null, string.Empty);
                await _matchmakingService.JoinMatchAsync(match.Id, candidate.UserId, null, string.Empty);
                entry.Status = RankedQueueStatus.Matched;
                entry.MatchId = match.Id;
                entry.CompletedAt = Clock.Now;
                candidate.Status = RankedQueueStatus.Matched;
                candidate.MatchId = match.Id;
                candidate.CompletedAt = Clock.Now;
            }

            return MapQueue(entry);
        }

        public async Task CancelRankedAsync(CancelRankedInput input)
        {
            var entry = await _queueRepository.GetAsync(input.QueueEntryId);
            if (entry.UserId != AbpSession.UserId)
            {
                throw new Abp.Authorization.AbpAuthorizationException("Only the queue owner can cancel it.");
            }

            if (entry.Status == RankedQueueStatus.Waiting)
            {
                entry.Status = RankedQueueStatus.Cancelled;
                entry.CompletedAt = Clock.Now;
            }
        }

        public async Task<RankedStatusDto> GetRankedStatusAsync(Guid gameId, string mode)
        {
            var userId = AbpSession.UserId ?? throw new Abp.Authorization.AbpAuthorizationException("Authentication required.");
            var rating = await _ratingRepository.FirstOrDefaultAsync(r => r.GameId == gameId && r.Mode == mode && r.UserId == userId);
            var queue = await _queueRepository.FirstOrDefaultAsync(q => q.GameId == gameId && q.Mode == mode && q.UserId == userId && q.Status == RankedQueueStatus.Waiting);
            return new RankedStatusDto
            {
                Rating = rating?.Rating ?? 1000,
                GamesPlayed = rating?.GamesPlayed ?? 0,
                Wins = rating?.Wins ?? 0,
                Losses = rating?.Losses ?? 0,
                Draws = rating?.Draws ?? 0,
                Queue = queue == null ? null : MapQueue(queue)
            };
        }

        public async Task<List<MatchHistoryDto>> GetMatchHistoryAsync(Guid gameId, int maxResultCount = 20)
        {
            var userId = AbpSession.UserId ?? throw new Abp.Authorization.AbpAuthorizationException("Authentication required.");
            var histories = await _historyRepository.GetAll()
                .Where(history => history.GameId == gameId && history.ResultsJson.Contains(userId.ToString()))
                .OrderByDescending(history => history.EndedAt)
                .Take(Math.Min(Math.Max(maxResultCount, 1), 100))
                .ToListAsync();
            var ids = histories.Select(history => history.Id).ToList();
            var replays = await _replayRepository.GetAll().Where(replay => ids.Contains(replay.MatchHistoryId)).ToDictionaryAsync(replay => replay.MatchHistoryId);
            return histories.Select(history => new MatchHistoryDto
            {
                MatchId = history.MatchId,
                GameId = history.GameId,
                Mode = history.Mode,
                Status = history.Status,
                WinnerUserId = history.WinnerUserId,
                StartedAt = history.StartedAt,
                EndedAt = history.EndedAt,
                ResultsJson = history.ResultsJson,
                ReplayEventCount = replays.TryGetValue(history.Id, out var replay) ? replay.EventCount : 0,
                ReplayDurationSeconds = replay?.DurationSeconds ?? 0
            }).ToList();
        }

        public async Task CompleteMatchAsync(CompleteMatchInput input)
        {
            var userId = AbpSession.UserId ?? throw new Abp.Authorization.AbpAuthorizationException("Authentication required.");
            var match = await _matchRepository.GetAll().Include(item => item.Participants).FirstOrDefaultAsync(item => item.Id == input.MatchId);
            if (match == null || match.Participants.All(participant => participant.UserId != userId))
            {
                throw new Abp.Authorization.AbpAuthorizationException("Only match participants can complete a match.");
            }

            if (input.ResultsJson?.Length > MatchmakingService.MaxPayloadBytes)
            {
                throw new InvalidOperationException("Match results exceed the maximum size.");
            }

            if (await _historyRepository.FirstOrDefaultAsync(history => history.MatchId == input.MatchId) != null)
            {
                return;
            }

            match.End();
            match.CompletedAt = Clock.Now;
            var history = await _historyRepository.InsertAsync(new MatchHistory
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                MatchId = match.Id,
                GameId = match.GameId,
                SeasonId = match.RankedSeasonId,
                Mode = match.Mode,
                Status = input.Status,
                WinnerUserId = input.WinnerUserId,
                StartedAt = match.StartedAt ?? match.CreationTime,
                EndedAt = Clock.Now,
                ResultsJson = input.ResultsJson ?? "{}"
            });

            await _replayRepository.InsertAsync(new ReplayMetadata
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                MatchHistoryId = history.Id,
                EventCount = Math.Max(0, input.ReplayEventCount),
                DurationSeconds = Math.Max(0, input.ReplayDurationSeconds),
                ExpiresAt = Clock.Now.AddDays(30)
            });
        }

        private static RankedQueueDto MapQueue(RankedQueueEntry entry)
        {
            return new RankedQueueDto
            {
                QueueEntryId = entry.Id,
                GameId = entry.GameId,
                SeasonId = entry.SeasonId,
                Mode = entry.Mode,
                Region = entry.Region,
                RatingSnapshot = entry.RatingSnapshot,
                Status = entry.Status,
                MatchId = entry.MatchId,
                EnqueuedAt = entry.EnqueuedAt
            };
        }
    }
}
