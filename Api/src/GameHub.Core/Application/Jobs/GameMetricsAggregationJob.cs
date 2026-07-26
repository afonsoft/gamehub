using Abp;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using GameHub.Catalog;
using GameHub.Gameplay;
using GameHub.Moderation;
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
        private readonly IRepository<UserContent, Guid> _userContentRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public GameMetricsAggregationJob(
            IRepository<PlaySession, Guid> playSessionRepository,
            IRepository<GameplayEvent, Guid> gameplayEventRepository,
            IRepository<GameMetricSnapshot, Guid> metricSnapshotRepository,
            IRepository<UserContent, Guid> userContentRepository,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _playSessionRepository = playSessionRepository;
            _gameplayEventRepository = gameplayEventRepository;
            _metricSnapshotRepository = metricSnapshotRepository;
            _userContentRepository = userContentRepository;
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

                var userContents = await _userContentRepository.GetAll()
                    .Where(c => c.CreationTime >= start && c.CreationTime < end && c.ContentType == UserContentType.Review && c.IsApproved && !c.RequiresModeration)
                    .ToListAsync();

                var sessionGroups = sessions
                    .GroupBy(s => s.GameId)
                    .ToDictionary(g => g.Key, g => g.AsEnumerable());

                var eventGroups = events
                    .GroupBy(e => e.GameId)
                    .ToDictionary(g => g.Key, g => g.AsEnumerable());

                var contentGroups = userContents
                    .GroupBy(c => c.GameId)
                    .ToDictionary(g => g.Key, g => g.AsEnumerable());

                var gameIds = sessionGroups.Keys.Union(eventGroups.Keys).Union(contentGroups.Keys).ToList();

                foreach (var gameId in gameIds)
                {
                    sessionGroups.TryGetValue(gameId, out var gameSessions);
                    eventGroups.TryGetValue(gameId, out var gameEvents);
                    contentGroups.TryGetValue(gameId, out var gameContents);

                    var (snapshot, isExisting) = await BuildSnapshotAsync(gameId, date, gameSessions, gameEvents, gameContents);
                    await UpsertSnapshotAsync(snapshot, isExisting);
                }

                await uow.CompleteAsync();
            }
        }

        private async Task<(GameMetricSnapshot Snapshot, bool IsExisting)> BuildSnapshotAsync(
            Guid gameId,
            DateTime date,
            IEnumerable<PlaySession> sessions,
            IEnumerable<GameplayEvent> events,
            IEnumerable<UserContent> userContents)
        {
            var sessionList = sessions?.ToList() ?? new List<PlaySession>();
            var productionSessions = sessionList.Where(s => !s.IsPlaytest).ToList();
            var eventList = events?.ToList() ?? new List<GameplayEvent>();
            var contentList = userContents?.ToList() ?? new List<UserContent>();

            var reviewsWithRating = contentList.Where(c => c.Rating.HasValue).ToList();
            var averageRating = reviewsWithRating.Any() ? reviewsWithRating.Average(c => c.Rating.Value) : (double?)null;
            var reviewCount = contentList.Count;

            var uniquePlayerKeys = productionSessions
                .Select(s => s.UserId.HasValue ? $"u:{s.UserId.Value}" : $"a:{s.AnonymousIdHash}")
                .Where(k => !string.IsNullOrEmpty(k))
                .Distinct()
                .ToList();

            var avgDuration = productionSessions.Any(s => s.EndedAt.HasValue)
                ? productionSessions
                    .Where(s => s.EndedAt.HasValue)
                    .Average(s => (s.EndedAt.Value - s.StartedAt).TotalSeconds)
                : 0.0;

            var medianDuration = ComputeMedianDuration(productionSessions);
            var dropOffRate = ComputeOnboardingDropOffRate(productionSessions);
            var (fpsAcceptable, fpsTotal) = ComputeFpsCounts(productionSessions);

            var pageViews = eventList.Count(e => e.EventType == GameplayEventType.GamePageViewed);
            var loadingStarted = eventList.Count(e => e.EventType == GameplayEventType.GameLoadingStarted);
            var loadingFinished = eventList.Count(e => e.EventType == GameplayEventType.GameLoadingFinished);
            var gameplayStarted = eventList.Count(e => e.EventType == GameplayEventType.GameplayStarted);
            var errors = eventList.Count(e => e.EventType == GameplayEventType.GameErrorCaptured);
            var commercialBreaks = eventList.Count(e => e.EventType == GameplayEventType.CommercialBreakCompleted);
            var rewardedBreaks = eventList.Count(e => e.EventType == GameplayEventType.RewardedBreakCompleted);

            var existing = (await _metricSnapshotRepository.GetAll()
                .Where(s => s.GameId == gameId)
                .ToListAsync())
                .FirstOrDefault(s => s.Date == date);

            if (existing != null)
            {
                existing.Plays = productionSessions.Count;
                existing.UniquePlayers = uniquePlayerKeys.Count;
                existing.DailyPlayingUsers = uniquePlayerKeys.Count;
                existing.PageViews = pageViews;
                existing.AvgDurationSeconds = avgDuration;
                existing.MedianSessionDurationSeconds = medianDuration;
                existing.OnboardingDropOffRate = dropOffRate;
                existing.LoadingStartedCount = loadingStarted;
                existing.LoadingFinishedCount = loadingFinished;
                existing.GameplayStartedCount = gameplayStarted;
                existing.ErrorCount = errors;
                existing.CommercialBreakCount = commercialBreaks;
                existing.RewardedBreakCount = rewardedBreaks;
                existing.FpsAcceptableSessions = fpsAcceptable;
                existing.FpsTotalSessions = fpsTotal;
                existing.AverageRating = averageRating;
                existing.ReviewCount = reviewCount;
                return (existing, true);
            }

            var snapshot = new GameMetricSnapshot
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                Date = date,
                Plays = productionSessions.Count,
                UniquePlayers = uniquePlayerKeys.Count,
                DailyPlayingUsers = uniquePlayerKeys.Count,
                PageViews = pageViews,
                AvgDurationSeconds = avgDuration,
                MedianSessionDurationSeconds = medianDuration,
                OnboardingDropOffRate = dropOffRate,
                LoadingStartedCount = loadingStarted,
                LoadingFinishedCount = loadingFinished,
                GameplayStartedCount = gameplayStarted,
                ErrorCount = errors,
                CommercialBreakCount = commercialBreaks,
                RewardedBreakCount = rewardedBreaks,
                FpsAcceptableSessions = fpsAcceptable,
                FpsTotalSessions = fpsTotal,
                AverageRating = averageRating,
                ReviewCount = reviewCount
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

        private static double ComputeMedianDuration(List<PlaySession> sessions)
        {
            var durations = sessions
                .Where(s => s.EndedAt.HasValue)
                .Select(s => (s.EndedAt.Value - s.StartedAt).TotalSeconds)
                .OrderBy(d => d)
                .ToList();

            if (durations.Count == 0)
            {
                return 0.0;
            }

            var mid = durations.Count / 2;
            return durations.Count % 2 == 0
                ? (durations[mid - 1] + durations[mid]) / 2.0
                : durations[mid];
        }

        private static double ComputeOnboardingDropOffRate(List<PlaySession> sessions)
        {
            if (sessions.Count == 0)
            {
                return 0.0;
            }

            var droppedOff = sessions.Count(s =>
                !s.EndedAt.HasValue
                || (s.EndedAt.Value - s.StartedAt).TotalSeconds < 60);

            return (double)droppedOff / sessions.Count;
        }

        private static (long Acceptable, long Total) ComputeFpsCounts(List<PlaySession> sessions)
        {
            var withFps = sessions.Where(s => s.FpsAverage.HasValue).ToList();
            if (withFps.Count == 0)
            {
                return (0, 0);
            }

            var acceptable = withFps.Count(s => s.FpsAverage.Value >= 30);
            return (acceptable, withFps.Count);
        }
    }
}
