using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using GameHub.Authorization;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Gameplay.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Gameplay
{
    /// <summary>
    /// Expõe métricas detalhadas de um jogo para desenvolvedores e administradores.
    /// </summary>
    [AbpAuthorize(GameHubPermissions.Pages_Developer_Games)]
    public class GameMetricsAppService : GameHubAppServiceBase, IGameMetricsAppService
    {
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<PlaySession, Guid> _playSessionRepository;
        private readonly IRepository<GameplayEvent, Guid> _gameplayEventRepository;

        public GameMetricsAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<PlaySession, Guid> playSessionRepository,
            IRepository<GameplayEvent, Guid> gameplayEventRepository)
        {
            _gameRepository = gameRepository;
            _playSessionRepository = playSessionRepository;
            _gameplayEventRepository = gameplayEventRepository;
        }

        public async Task<GameMetricsResult> GetMetricsAsync(Guid gameId, GameMetricsFilter input)
        {
            await EnsureGameAccessAsync(gameId);
            ValidateDateRange(input);

            var start = input.From?.Date ?? Clock.Now.AddDays(-30).Date;
            var end = input.To?.Date.AddDays(1) ?? Clock.Now.AddDays(1).Date;

            var sessionsQuery = _playSessionRepository.GetAll()
                .Where(s => s.GameId == gameId && s.StartedAt >= start && s.StartedAt < end);

            if (!string.IsNullOrWhiteSpace(input.CountryCode))
            {
                sessionsQuery = sessionsQuery.Where(s => s.CountryCode == input.CountryCode);
            }

            if (!string.IsNullOrWhiteSpace(input.DeviceType))
            {
                sessionsQuery = sessionsQuery.Where(s => s.DeviceType == input.DeviceType);
            }

            var sessions = await sessionsQuery.ToListAsync();

            var sessionIds = sessions.Select(s => s.Id).ToList();
            var eventsQuery = _gameplayEventRepository.GetAll()
                .Where(e => e.GameId == gameId && e.OccurredAt >= start && e.OccurredAt < end);

            if (sessionIds.Any())
            {
                eventsQuery = eventsQuery.Where(e => sessionIds.Contains(e.PlaySessionId));
            }

            var events = await eventsQuery.ToListAsync();

            var sessionGroups = sessions
                .GroupBy(s => s.StartedAt.Date)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var eventGroups = events
                .GroupBy(e => e.OccurredAt.Date)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var allDates = sessionGroups.Keys.Union(eventGroups.Keys).OrderBy(d => d).ToList();

            var daily = new List<GameMetricsDailyItemDto>();
            foreach (var date in allDates)
            {
                sessionGroups.TryGetValue(date, out var daySessions);
                eventGroups.TryGetValue(date, out var dayEvents);
                daily.Add(BuildDailyItem(date, daySessions, dayEvents));
            }

            var endedSessions = sessions.Where(s => s.EndedAt.HasValue).ToList();

            return new GameMetricsResult
            {
                TotalPlays = sessions.Count,
                TotalUniquePlayers = CountUniquePlayers(sessions),
                AverageDurationSeconds = endedSessions.Any()
                    ? endedSessions.Average(s => (s.EndedAt.Value - s.StartedAt).TotalSeconds)
                    : 0.0,
                LoadingFinishedCount = events.Count(e => e.EventType == GameplayEventType.GameLoadingFinished),
                ErrorCount = events.Count(e => e.EventType == GameplayEventType.GameErrorCaptured),
                CommercialBreakCount = events.Count(e => e.EventType == GameplayEventType.CommercialBreakCompleted),
                RewardedBreakCount = events.Count(e => e.EventType == GameplayEventType.RewardedBreakCompleted),
                Daily = daily
            };
        }

        private static void ValidateDateRange(GameMetricsFilter input)
        {
            if (input.From.HasValue && input.To.HasValue && input.From.Value.Date > input.To.Value.Date)
            {
                throw new ArgumentException("The metrics period start cannot be after its end.", nameof(input));
            }
        }

        private async Task EnsureGameAccessAsync(Guid gameId)
        {
            if (!AbpSession.UserId.HasValue)
            {
                throw new AbpAuthorizationException("User is not authenticated.");
            }

            var game = await _gameRepository.GetAll()
                .Where(g => g.Id == gameId && !g.IsDeleted)
                .Include(g => g.DeveloperProfile)
                .FirstOrDefaultAsync();

            if (game == null)
            {
                throw new AbpAuthorizationException("Game not found.");
            }

            var isAdmin = PermissionChecker.IsGranted(GameHubPermissions.Pages_Dashboard_View);
            var isOwner = game.DeveloperProfile != null && game.DeveloperProfile.UserId == AbpSession.UserId.Value;

            if (!isOwner && !isAdmin)
            {
                throw new AbpAuthorizationException("You do not have access to this game's metrics.");
            }
        }

        private static long CountUniquePlayers(IEnumerable<PlaySession> sessions)
        {
            return sessions
                .Select(s => s.UserId.HasValue ? $"u:{s.UserId.Value}" : $"a:{s.AnonymousIdHash}")
                .Where(k => k != "a:")
                .Distinct()
                .Count();
        }

        private static GameMetricsDailyItemDto BuildDailyItem(DateTime date, IEnumerable<PlaySession> sessions, IEnumerable<GameplayEvent> events)
        {
            var sessionList = sessions?.ToList() ?? new List<PlaySession>();
            var eventList = events?.ToList() ?? new List<GameplayEvent>();
            var endedSessions = sessionList.Where(s => s.EndedAt.HasValue).ToList();

            return new GameMetricsDailyItemDto
            {
                Date = date,
                Plays = sessionList.Count,
                UniquePlayers = CountUniquePlayers(sessionList),
                AvgDurationSeconds = endedSessions.Any()
                    ? endedSessions.Average(s => (s.EndedAt.Value - s.StartedAt).TotalSeconds)
                    : 0.0,
                LoadingFinishedCount = eventList.Count(e => e.EventType == GameplayEventType.GameLoadingFinished),
                ErrorCount = eventList.Count(e => e.EventType == GameplayEventType.GameErrorCaptured),
                CommercialBreakCount = eventList.Count(e => e.EventType == GameplayEventType.CommercialBreakCompleted),
                RewardedBreakCount = eventList.Count(e => e.EventType == GameplayEventType.RewardedBreakCompleted)
            };
        }
    }
}
