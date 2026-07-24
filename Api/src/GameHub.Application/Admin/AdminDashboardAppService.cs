using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using Eaf.Middleware.Authorization.Users;
using GameHub.Admin.Dto;
using GameHub.Authorization;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Gameplay;
using GameHub.Moderation;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Admin
{
    /// <summary>
    /// Expõe métricas e séries temporais para o dashboard administrativo.
    /// </summary>
    [AbpAuthorize(GameHubPermissions.Pages_Dashboard_View)]
    public class AdminDashboardAppService : GameHubAppServiceBase, IAdminDashboardAppService
    {
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<GameBuild, Guid> _buildRepository;
        private readonly IRepository<ModerationReview, Guid> _reviewRepository;
        private readonly IRepository<PlaySession, Guid> _playSessionRepository;
        private readonly IRepository<GameplayEvent, Guid> _gameplayEventRepository;
        private readonly IRepository<GameMetricSnapshot, Guid> _metricSnapshotRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<DeveloperProfile, Guid> _developerProfileRepository;

        public AdminDashboardAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<GameBuild, Guid> buildRepository,
            IRepository<ModerationReview, Guid> reviewRepository,
            IRepository<PlaySession, Guid> playSessionRepository,
            IRepository<GameplayEvent, Guid> gameplayEventRepository,
            IRepository<GameMetricSnapshot, Guid> metricSnapshotRepository,
            IRepository<User, long> userRepository,
            IRepository<DeveloperProfile, Guid> developerProfileRepository)
        {
            _gameRepository = gameRepository;
            _buildRepository = buildRepository;
            _reviewRepository = reviewRepository;
            _playSessionRepository = playSessionRepository;
            _gameplayEventRepository = gameplayEventRepository;
            _metricSnapshotRepository = metricSnapshotRepository;
            _userRepository = userRepository;
            _developerProfileRepository = developerProfileRepository;
        }

        public async Task<AdminDashboardSummaryDto> GetSummaryAsync()
        {
            var totalGames = await _gameRepository.CountAsync(g => !g.IsDeleted);
            var pendingReviews = await _reviewRepository.CountAsync(r => r.Status == ModerationReviewStatus.Pending && !r.IsDeleted);
            var totalPlays = await _gameRepository.GetAll()
                .Where(g => !g.IsDeleted)
                .SumAsync(g => (long?)g.TotalPlays) ?? 0L;

            var activeSince = DateTime.UtcNow.AddDays(-7);
            var activeUsers = await _playSessionRepository.GetAll()
                .Where(s => s.StartedAt >= activeSince && s.UserId.HasValue)
                .Select(s => s.UserId)
                .Distinct()
                .CountAsync();

            var totalUsers = await _userRepository.CountAsync(u => !u.IsDeleted);
            var totalDevelopers = await _developerProfileRepository.CountAsync();
            var totalBuilds = await _buildRepository.CountAsync(b => !b.IsDeleted);
            var pendingUploads = await _buildRepository.CountAsync(b => b.Status == GameBuildStatus.Uploaded && !b.IsDeleted);

            return new AdminDashboardSummaryDto
            {
                TotalGames = totalGames,
                PendingReviews = pendingReviews,
                TotalPlays = totalPlays,
                ActiveUsers7d = activeUsers,
                TotalUsers = totalUsers,
                TotalDevelopers = totalDevelopers,
                TotalBuilds = totalBuilds,
                PendingUploads = pendingUploads,
            };
        }

        public async Task<PlaysOverTimeResultDto> GetPlaysOverTimeAsync(int days)
        {
            if (days < 1)
            {
                days = 30;
            }

            var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);
            var snapshots = await _metricSnapshotRepository.GetAll()
                .Where(s => s.Date >= startDate)
                .GroupBy(s => s.Date)
                .Select(g => new PlaysOverTimeItemDto
                {
                    Date = g.Key,
                    Plays = g.Sum(x => x.Plays)
                })
                .ToListAsync();

            if (snapshots.Any())
            {
                return new PlaysOverTimeResultDto { Items = snapshots.OrderBy(i => i.Date).ToList() };
            }

            var fallback = await _playSessionRepository.GetAll()
                .Where(s => s.StartedAt >= startDate)
                .GroupBy(s => s.StartedAt.Date)
                .Select(g => new PlaysOverTimeItemDto
                {
                    Date = g.Key,
                    Plays = g.Count()
                })
                .ToListAsync();

            var allDates = Enumerable.Range(0, days)
                .Select(d => startDate.AddDays(d))
                .ToList();

            var lookup = fallback.ToDictionary(x => x.Date);
            var result = allDates.Select(date =>
                lookup.TryGetValue(date, out var item)
                    ? item
                    : new PlaysOverTimeItemDto { Date = date, Plays = 0 })
                .ToList();

            return new PlaysOverTimeResultDto { Items = result };
        }

        public async Task<ListResultDto<AdminBuildListItemDto>> GetRecentUploadsAsync(int count)
        {
            if (count < 1) count = 5;
            if (count > 50) count = 50;

            var items = await _buildRepository.GetAll()
                .Where(b => !b.IsDeleted)
                .Include(b => b.Game)
                    .ThenInclude(g => g.DeveloperProfile)
                .OrderByDescending(b => b.CreationTime)
                .Take(count)
                .ToListAsync();

            return new ListResultDto<AdminBuildListItemDto>(items.Select(MapToBuildListItem).ToList());
        }

        public async Task<ListResultDto<AdminGameListItemDto>> GetRecentGamesAsync(int count)
        {
            if (count < 1) count = 5;
            if (count > 50) count = 50;

            var items = await _gameRepository.GetAll()
                .Where(g => !g.IsDeleted)
                .Include(g => g.DeveloperProfile)
                .OrderByDescending(g => g.CreationTime)
                .Take(count)
                .ToListAsync();

            return new ListResultDto<AdminGameListItemDto>(ObjectMapper.Map<List<AdminGameListItemDto>>(items));
        }

        public async Task<ListResultDto<AdminGameListItemDto>> GetTopGamesAsync(int count)
        {
            if (count < 1) count = 5;
            if (count > 50) count = 50;

            var items = await _gameRepository.GetAll()
                .Where(g => !g.IsDeleted)
                .Include(g => g.DeveloperProfile)
                .OrderByDescending(g => g.TotalPlays)
                .Take(count)
                .ToListAsync();

            return new ListResultDto<AdminGameListItemDto>(ObjectMapper.Map<List<AdminGameListItemDto>>(items));
        }

        public async Task<ListResultDto<ModerationReviewDto>> GetPendingReviewsAsync(int count)
        {
            if (count < 1) count = 5;
            if (count > 50) count = 50;

            var items = await _reviewRepository.GetAll()
                .Where(r => r.Status == ModerationReviewStatus.Pending && !r.IsDeleted)
                .Include(r => r.Game)
                .OrderByDescending(r => r.CreationTime)
                .Take(count)
                .ToListAsync();

            return new ListResultDto<ModerationReviewDto>(ObjectMapper.Map<List<ModerationReviewDto>>(items));
        }

        public async Task<AdminMetricsSummaryDto> GetMetricsAsync(DateTime? startDate, DateTime? endDate)
        {
            var end = endDate?.Date ?? Clock.Now.Date;
            var start = startDate?.Date ?? end.AddDays(-29);

            var startAt = start;
            var endAt = end.AddDays(1).AddTicks(-1);

            var sessions = await _playSessionRepository.GetAll()
                .Where(s => s.StartedAt >= startAt && s.StartedAt <= endAt)
                .Select(s => new { s.UserId, s.AnonymousIdHash, s.StartedAt, s.EndedAt, s.DeviceType, s.Browser, s.CountryCode, s.FpsAverage, s.FpsMin })
                .ToListAsync();

            var events = await _gameplayEventRepository.GetAll()
                .Where(e => e.OccurredAt >= startAt && e.OccurredAt <= endAt)
                .Select(e => new { e.GameId, e.EventType })
                .ToListAsync();

            var totalPlays = sessions.Count;
            var avgDuration = sessions
                .Where(s => s.EndedAt.HasValue)
                .Select(s => (s.EndedAt.Value - s.StartedAt).TotalSeconds)
                .DefaultIfEmpty(0)
                .Average();

            var today = Clock.Now.Date;
            var monthlySince = today.AddDays(-30);
            var dailyActiveUsers = sessions
                .Where(s => s.StartedAt.Date == today)
                .Select(s => s.UserId?.ToString() ?? s.AnonymousIdHash)
                .Distinct()
                .Count();
            var monthlyActiveUsers = sessions
                .Where(s => s.StartedAt.Date >= monthlySince)
                .Select(s => s.UserId?.ToString() ?? s.AnonymousIdHash)
                .Distinct()
                .Count();

            var loadingStarted = events.Count(e => e.EventType == GameplayEventType.GameLoadingStarted);
            var loadingFinished = events.Count(e => e.EventType == GameplayEventType.GameLoadingFinished);
            var conversionRate = loadingStarted > 0 ? (double)loadingFinished / loadingStarted : 0;

            var gameplayStarted = events.Count(e => e.EventType == GameplayEventType.GameplayStarted);
            var errors = events.Count(e => e.EventType == GameplayEventType.GameErrorCaptured);
            var errorRate = gameplayStarted > 0 ? (double)errors / gameplayStarted : 0;

            var fpsSessions = sessions.Where(s => s.FpsAverage.HasValue).ToList();
            var averageFps = fpsSessions.Any() ? fpsSessions.Average(s => s.FpsAverage.Value) : (double?)null;
            var minFps = fpsSessions.Any() ? fpsSessions.Min(s => s.FpsMin ?? s.FpsAverage.Value) : (double?)null;

            return new AdminMetricsSummaryDto
            {
                StartDate = start,
                EndDate = end,
                TotalPlays = totalPlays,
                DailyActiveUsers = dailyActiveUsers,
                MonthlyActiveUsers = monthlyActiveUsers,
                AverageSessionDurationSeconds = avgDuration,
                LoadConversionRate = conversionRate,
                ErrorRate = errorRate,
                Devices = BuildDistribution(sessions.Select(s => s.DeviceType)),
                Countries = BuildDistribution(sessions.Select(s => s.CountryCode ?? "Unknown")),
                Browsers = BuildDistribution(sessions.Select(s => s.Browser)),
                AverageFps = averageFps,
                MinimumFps = minFps
            };
        }

        public async Task<List<AdminHealthAlertDto>> GetHealthAlertsAsync()
        {
            var since = Clock.Now.AddDays(-7);
            var events = await _gameplayEventRepository.GetAll()
                .Where(e => e.OccurredAt >= since)
                .Select(e => new { e.GameId, e.Game.Title, e.EventType })
                .ToListAsync();

            var alerts = new List<AdminHealthAlertDto>();
            var grouped = events.GroupBy(e => e.GameId);

            foreach (var group in grouped)
            {
                var title = group.First().Title;
                var started = group.Count(e => e.EventType == GameplayEventType.GameLoadingStarted);
                var finished = group.Count(e => e.EventType == GameplayEventType.GameLoadingFinished);
                var gameplay = group.Count(e => e.EventType == GameplayEventType.GameplayStarted);
                var errors = group.Count(e => e.EventType == GameplayEventType.GameErrorCaptured);

                if (started > 0 && finished * 2 < started)
                {
                    alerts.Add(new AdminHealthAlertDto
                    {
                        GameId = group.Key,
                        GameTitle = title,
                        Reason = "Load conversion below 50%",
                        Severity = "Critical",
                        MetricValue = started > 0 ? (double)finished / started : 0
                    });
                }

                if (gameplay > 0 && errors * 10 > gameplay)
                {
                    alerts.Add(new AdminHealthAlertDto
                    {
                        GameId = group.Key,
                        GameTitle = title,
                        Reason = "Error rate above 10%",
                        Severity = "Warning",
                        MetricValue = (double)errors / gameplay
                    });
                }
            }

            var fpsSessions = await _playSessionRepository.GetAll()
                .Where(s => s.StartedAt >= since)
                .Where(s => s.FpsAverage.HasValue || s.FpsMin.HasValue)
                .Select(s => new { s.GameId, s.Game.Title, s.FpsAverage, s.FpsMin })
                .ToListAsync();

            var fpsGroups = fpsSessions.GroupBy(s => s.GameId);
            foreach (var group in fpsGroups)
            {
                var title = group.First().Title;
                var sessionsWithMinFps = group.Where(s => s.FpsMin.HasValue).ToList();
                var lowMinCount = sessionsWithMinFps.Count(s => s.FpsMin < 30);
                var totalMin = sessionsWithMinFps.Count;
                if (totalMin > 0 && (double)lowMinCount / totalMin > 0.05)
                {
                    alerts.Add(new AdminHealthAlertDto
                    {
                        GameId = group.Key,
                        GameTitle = title,
                        Reason = "Min FPS below 30 in more than 5% of sessions",
                        Severity = "Warning",
                        MetricValue = (double)lowMinCount / totalMin
                    });
                }

                var lowAvgCount = group.Count(s => s.FpsAverage < 50);
                var totalAvg = group.Count();
                if (totalAvg > 0 && (double)lowAvgCount / totalAvg > 0.2)
                {
                    alerts.Add(new AdminHealthAlertDto
                    {
                        GameId = group.Key,
                        GameTitle = title,
                        Reason = "Average FPS below 50 in more than 20% of sessions",
                        Severity = "Warning",
                        MetricValue = (double)lowAvgCount / totalAvg
                    });
                }
            }

            return alerts.OrderByDescending(a => a.MetricValue).ToList();
        }

        private static List<MetricDistributionItemDto> BuildDistribution(IEnumerable<string> values)
        {
            var filtered = values.Select(v => string.IsNullOrWhiteSpace(v) ? "Unknown" : v).ToList();
            var total = filtered.Count;
            if (total == 0)
            {
                return new List<MetricDistributionItemDto>();
            }

            return filtered
                .GroupBy(v => v)
                .Select(g => new MetricDistributionItemDto
                {
                    Name = g.Key,
                    Count = g.Count(),
                    Percentage = (double)g.Count() / total
                })
                .OrderByDescending(x => x.Count)
                .ToList();
        }

        private static AdminBuildListItemDto MapToBuildListItem(GameBuild build)
        {
            return new AdminBuildListItemDto
            {
                Id = build.Id,
                GameId = build.GameId,
                GameTitle = build.Game?.Title ?? string.Empty,
                DeveloperName = build.Game?.DeveloperProfile?.DisplayName ?? string.Empty,
                Version = build.Version,
                BuildNumber = build.BuildNumber,
                Status = build.Status.ToString(),
                SizeBytes = build.SizeBytes,
                HashSha256 = build.HashSha256,
                CreatedAt = build.CreationTime,
                PublishedAt = build.PublishedTime
            };
        }
    }
}
