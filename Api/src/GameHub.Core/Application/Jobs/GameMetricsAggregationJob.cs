using Abp;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using GameHub.Catalog;
using GameHub.Gameplay;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameHub.Jobs
{
    /// <summary>
    /// Aggregates gameplay sessions and events into daily GameMetricSnapshot rows.
    /// </summary>
    public class GameMetricsAggregationJob : ITransientDependency
    {
        private readonly IRepository<PlaySession, Guid> _playSessionRepository;
        private readonly IRepository<GameplayEvent, Guid> _gameplayEventRepository;
        private readonly IRepository<GameMetricSnapshot, Guid> _metricSnapshotRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public GameMetricsAggregationJob(
            IRepository<PlaySession, Guid> playSessionRepository,
            IRepository<GameplayEvent, Guid> gameplayEventRepository,
            IRepository<GameMetricSnapshot, Guid> metricSnapshotRepository,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _playSessionRepository = playSessionRepository;
            _gameplayEventRepository = gameplayEventRepository;
            _metricSnapshotRepository = metricSnapshotRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task Execute(GameMetricsAggregationArgs args)
        {
            var date = args.Date.Date;
            var start = date;
            var end = date.AddDays(1);

            using (var uow = _unitOfWorkManager.Begin())
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var sessions = await _playSessionRepository.GetAll()
                    .Where(s => s.StartedAt >= start && s.StartedAt < end)
                    .ToListAsync();

                var events = await _gameplayEventRepository.GetAll()
                    .Where(e => e.OccurredAt >= start && e.OccurredAt < end)
                    .ToListAsync();

                var sessionGroups = sessions
                    .GroupBy(s => s.GameId)
                    .ToDictionary(g => g.Key, g => g.AsEnumerable());

                var eventGroups = events
                    .GroupBy(e => e.GameId)
                    .ToDictionary(g => g.Key, g => g.AsEnumerable());

                var gameIds = sessionGroups.Keys.Union(eventGroups.Keys).ToList();

                foreach (var gameId in gameIds)
                {
                    sessionGroups.TryGetValue(gameId, out var gameSessions);
                    eventGroups.TryGetValue(gameId, out var gameEvents);

                    var (snapshot, isExisting) = await BuildSnapshotAsync(gameId, date, gameSessions, gameEvents);
                    await UpsertSnapshotAsync(snapshot, isExisting);
                }

                await uow.CompleteAsync();
            }
        }

        private async Task<(GameMetricSnapshot Snapshot, bool IsExisting)> BuildSnapshotAsync(
            Guid gameId,
            DateTime date,
            IEnumerable<PlaySession> sessions,
            IEnumerable<GameplayEvent> events)
        {
            var sessionList = sessions?.ToList() ?? new List<PlaySession>();
            var eventList = events?.ToList() ?? new List<GameplayEvent>();

            var uniquePlayerKeys = sessionList
                .Select(s => s.UserId.HasValue ? $"u:{s.UserId.Value}" : $"a:{s.AnonymousIdHash}")
                .Where(k => !string.IsNullOrEmpty(k))
                .Distinct()
                .ToList();

            var avgDuration = sessionList.Any(s => s.EndedAt.HasValue)
                ? sessionList
                    .Where(s => s.EndedAt.HasValue)
                    .Average(s => (s.EndedAt.Value - s.StartedAt).TotalSeconds)
                : 0.0;

            var loadingFinished = eventList.Count(e => e.EventType == GameplayEventType.GameLoadingFinished);
            var errors = eventList.Count(e => e.EventType == GameplayEventType.GameErrorCaptured);
            var commercialBreaks = eventList.Count(e => e.EventType == GameplayEventType.CommercialBreakCompleted);
            var rewardedBreaks = eventList.Count(e => e.EventType == GameplayEventType.RewardedBreakCompleted);

            var existing = (await _metricSnapshotRepository.GetAll()
                .Where(s => s.GameId == gameId)
                .ToListAsync())
                .FirstOrDefault(s => s.Date == date);

            if (existing != null)
            {
                existing.Plays = sessionList.Count;
                existing.UniquePlayers = uniquePlayerKeys.Count;
                existing.AvgDurationSeconds = avgDuration;
                existing.LoadingFinishedCount = loadingFinished;
                existing.ErrorCount = errors;
                existing.CommercialBreakCount = commercialBreaks;
                existing.RewardedBreakCount = rewardedBreaks;
                return (existing, true);
            }

            var snapshot = new GameMetricSnapshot
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                Date = date,
                Plays = sessionList.Count,
                UniquePlayers = uniquePlayerKeys.Count,
                AvgDurationSeconds = avgDuration,
                LoadingFinishedCount = loadingFinished,
                ErrorCount = errors,
                CommercialBreakCount = commercialBreaks,
                RewardedBreakCount = rewardedBreaks,
            };

            return (snapshot, false);
        }

        private async Task UpsertSnapshotAsync(GameMetricSnapshot snapshot, bool isExisting)
        {
            if (isExisting)
            {
                await _metricSnapshotRepository.UpdateAsync(snapshot);
            }
            else
            {
                await _metricSnapshotRepository.InsertAsync(snapshot);
            }
        }
    }
}
