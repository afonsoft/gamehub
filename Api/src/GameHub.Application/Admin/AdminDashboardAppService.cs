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
        private readonly IRepository<GameErrorLog, Guid> _errorLogRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<DeveloperProfile, Guid> _developerProfileRepository;

        public AdminDashboardAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<GameBuild, Guid> buildRepository,
            IRepository<ModerationReview, Guid> reviewRepository,
            IRepository<PlaySession, Guid> playSessionRepository,
            IRepository<GameplayEvent, Guid> gameplayEventRepository,
            IRepository<GameMetricSnapshot, Guid> metricSnapshotRepository,
            IRepository<GameErrorLog, Guid> errorLogRepository,
            IRepository<User, long> userRepository,
            IRepository<DeveloperProfile, Guid> developerProfileRepository)
        {
            _gameRepository = gameRepository;
            _buildRepository = buildRepository;
            _reviewRepository = reviewRepository;
            _playSessionRepository = playSessionRepository;
            _gameplayEventRepository = gameplayEventRepository;
            _metricSnapshotRepository = metricSnapshotRepository;
            _errorLogRepository = errorLogRepository;
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
                .Select(s => new { s.UserId, s.AnonymousIdHash, s.StartedAt, s.EndedAt, s.DeviceType, s.Browser, s.CountryCode, s.FpsAverage, s.FpsMin, s.IsPlaytest })
                .ToListAsync();

            var events = await _gameplayEventRepository.GetAll()
                .Where(e => e.OccurredAt >= startAt && e.OccurredAt <= endAt)
                .Select(e => new { e.GameId, e.EventType })
                .ToListAsync();

            var productionSessions = sessions.Where(s => !s.IsPlaytest).ToList();
            var totalPlays = productionSessions.Count;
            var durations = productionSessions
                .Where(s => s.EndedAt.HasValue)
                .Select(s => (s.EndedAt.Value - s.StartedAt).TotalSeconds)
                .ToList();
            var avgDuration = durations.Any() ? durations.Average() : 0.0;
            var medianDuration = ComputeMedian(durations);
            var dropOffRate = productionSessions.Count == 0
                ? 0.0
                : (double)productionSessions.Count(s => !s.EndedAt.HasValue || (s.EndedAt.Value - s.StartedAt).TotalSeconds < 60) / productionSessions.Count;

            var today = Clock.Now.Date;
            var monthlySince = today.AddDays(-30);
            var dailyActiveUsers = productionSessions
                .Where(s => s.StartedAt.Date == today)
                .Select(s => s.UserId?.ToString() ?? s.AnonymousIdHash)
                .Distinct()
                .Count();
            var monthlyActiveUsers = productionSessions
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

            var fpsSessions = productionSessions.Where(s => s.FpsAverage.HasValue).ToList();
            var averageFps = fpsSessions.Any() ? fpsSessions.Average(s => s.FpsAverage.Value) : (double?)null;
            var minFps = fpsSessions.Any() ? fpsSessions.Min(s => s.FpsMin ?? s.FpsAverage.Value) : (double?)null;
            var fpsAcceptable = fpsSessions.Count(s => s.FpsAverage >= 30);
            var fpsByDevice = BuildFpsByDevice(fpsSessions);

            return new AdminMetricsSummaryDto
            {
                StartDate = start,
                EndDate = end,
                TotalPlays = totalPlays,
                DailyActiveUsers = dailyActiveUsers,
                MonthlyActiveUsers = monthlyActiveUsers,
                AverageSessionDurationSeconds = avgDuration,
                MedianSessionDurationSeconds = medianDuration,
                OnboardingDropOffRate = dropOffRate,
                LoadConversionRate = conversionRate,
                ErrorRate = errorRate,
                Devices = BuildDistribution(productionSessions.Select(s => s.DeviceType)),
                Countries = BuildDistribution(productionSessions.Select(s => s.CountryCode ?? "Unknown")),
                Browsers = BuildDistribution(productionSessions.Select(s => s.Browser)),
                AverageFps = averageFps,
                MinimumFps = minFps,
                FpsAcceptableSessions = fpsAcceptable,
                FpsTotalSessions = fpsSessions.Count,
                FpsByDevice = fpsByDevice
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

            var deviceFpsSessions = await _playSessionRepository.GetAll()
                .Where(s => s.StartedAt >= since && (s.FpsAverage.HasValue || s.FpsMin.HasValue))
                .Select(s => new { s.GameId, s.Game.Title, s.DeviceType, s.FpsAverage })
                .ToListAsync();

            var deviceGroups = deviceFpsSessions.GroupBy(s => new { s.GameId, s.DeviceType });
            foreach (var group in deviceGroups)
            {
                var withFps = group.Where(s => s.FpsAverage.HasValue).ToList();
                if (withFps.Count < 5)
                {
                    continue;
                }

                var acceptable = withFps.Count(s => s.FpsAverage >= 30);
                var acceptableRate = (double)acceptable / withFps.Count;
                if (acceptableRate < 0.85)
                {
                    alerts.Add(new AdminHealthAlertDto
                    {
                        GameId = group.Key.GameId,
                        GameTitle = withFps.First().Title,
                        Reason = $"FPS acceptable rate below 85% on {group.Key.DeviceType}",
                        Severity = "Warning",
                        MetricValue = acceptableRate
                    });
                }
            }

            var lowRatingSnapshots = await _metricSnapshotRepository.GetAll()
                .Where(s => s.Date >= since && s.ReviewCount >= 10 && s.AverageRating < 3.0)
                .Select(s => new { s.GameId, s.Game.Title, s.AverageRating, s.ReviewCount })
                .ToListAsync();

            foreach (var snapshot in lowRatingSnapshots)
            {
                alerts.Add(new AdminHealthAlertDto
                {
                    GameId = snapshot.GameId,
                    GameTitle = snapshot.Title,
                    Reason = $"Average player rating below 3.0 over {snapshot.ReviewCount} reviews",
                    Severity = "Warning",
                    MetricValue = snapshot.AverageRating ?? 0
                });
            }

            return alerts.OrderByDescending(a => a.MetricValue).ToList();
        }

        public async Task<AdminOnboardingInsightsDto> GetOnboardingInsightsAsync(Guid gameId, DateTime? startDate, DateTime? endDate)
        {
            var game = await _gameRepository.GetAsync(gameId);
            var end = endDate?.Date ?? Clock.Now.Date;
            var start = startDate?.Date ?? end.AddDays(-29);
            var startAt = start;
            var endAt = end.AddDays(1).AddTicks(-1);

            var sessions = await _playSessionRepository.GetAll()
                .Where(s => s.GameId == gameId && s.StartedAt >= startAt && s.StartedAt <= endAt && !s.IsPlaytest)
                .Select(s => new { s.StartedAt, s.EndedAt, s.DeviceType, s.CountryCode })
                .ToListAsync();

            var dropOffByDevice = sessions
                .GroupBy(s => string.IsNullOrWhiteSpace(s.DeviceType) ? "Unknown" : s.DeviceType)
                .Select(g => new MetricDistributionItemDto
                {
                    Name = g.Key,
                    Count = g.Count(s => !s.EndedAt.HasValue || (s.EndedAt.Value - s.StartedAt).TotalSeconds < 60),
                    Percentage = g.Count() == 0 ? 0 : (double)g.Count(s => !s.EndedAt.HasValue || (s.EndedAt.Value - s.StartedAt).TotalSeconds < 60) / g.Count()
                })
                .ToList();

            var dropOffByCountry = sessions
                .GroupBy(s => string.IsNullOrWhiteSpace(s.CountryCode) ? "Unknown" : s.CountryCode)
                .Select(g => new MetricDistributionItemDto
                {
                    Name = g.Key,
                    Count = g.Count(s => !s.EndedAt.HasValue || (s.EndedAt.Value - s.StartedAt).TotalSeconds < 60),
                    Percentage = g.Count() == 0 ? 0 : (double)g.Count(s => !s.EndedAt.HasValue || (s.EndedAt.Value - s.StartedAt).TotalSeconds < 60) / g.Count()
                })
                .ToList();

            var overallDropOff = sessions.Count == 0 ? 0.0 : (double)sessions.Count(s => !s.EndedAt.HasValue || (s.EndedAt.Value - s.StartedAt).TotalSeconds < 60) / sessions.Count;
            var suggestions = new List<string>();
            if (overallDropOff > 0.25)
            {
                suggestions.Add("Adicione um botão de skip no tutorial para reduzir o abandono.");
            }
            if (dropOffByDevice.Any(d => d.Percentage > 0.35))
            {
                suggestions.Add("Verifique a experiência de carregamento no dispositivo com maior taxa de abandono.");
            }

            return new AdminOnboardingInsightsDto
            {
                GameId = gameId,
                GameTitle = game.Title,
                StartDate = start,
                EndDate = end,
                OverallDropOffRate = overallDropOff,
                DropOffByDevice = dropOffByDevice,
                DropOffByCountry = dropOffByCountry,
                Suggestions = suggestions
            };
        }

        public async Task<AdminEngagementInsightsDto> GetEngagementInsightsAsync(Guid gameId, DateTime? startDate, DateTime? endDate)
        {
            var game = await _gameRepository.GetAsync(gameId);
            var end = endDate?.Date ?? Clock.Now.Date;
            var start = startDate?.Date ?? end.AddDays(-29);
            var startAt = start;
            var endAt = end.AddDays(1).AddTicks(-1);

            var sessions = await _playSessionRepository.GetAll()
                .Where(s => s.GameId == gameId && s.StartedAt >= startAt && s.StartedAt <= endAt && !s.IsPlaytest && s.EndedAt.HasValue)
                .Select(s => (s.EndedAt.Value - s.StartedAt).TotalSeconds)
                .ToListAsync();

            var avg = sessions.Any() ? sessions.Average() : 0.0;
            var median = ComputeMedian(sessions);
            var benchmark = 120.0;
            var below = avg < benchmark;
            var suggestions = new List<string>();
            if (below)
            {
                suggestions.Add("Média de sessão abaixo de 2 minutos; considere adicionar metas diárias ou recompensas de retorno.");
            }
            if (median < 60)
            {
                suggestions.Add("Mediana de sessão muito curta; avalie o onboarding e o primeiro loop de jogo.");
            }

            return new AdminEngagementInsightsDto
            {
                GameId = gameId,
                GameTitle = game.Title,
                StartDate = start,
                EndDate = end,
                AverageSessionDurationSeconds = avg,
                MedianSessionDurationSeconds = median,
                BenchmarkSeconds = benchmark,
                BelowBenchmark = below,
                Suggestions = suggestions
            };
        }

        public async Task<ErrorScannerResultDto> GetErrorScannerAsync(Guid? gameId, Guid? buildId, int hours)
        {
            if (hours < 1) hours = 24;
            if (hours > 168) hours = 168;

            var end = Clock.Now;
            var start = end.AddHours(-hours);

            var query = _errorLogRepository.GetAll()
                .Where(e => e.Timestamp >= start && e.Timestamp <= end);

            if (gameId.HasValue)
            {
                query = query.Where(e => e.GameId == gameId.Value);
            }

            if (buildId.HasValue)
            {
                query = query.Where(e => e.BuildId == buildId.Value);
            }

            var logs = await query
                .OrderByDescending(e => e.Timestamp)
                .Take(1000)
                .Select(e => new { e.GameId, e.Game.Title, e.Message, e.Severity, e.Timestamp, e.StackTrace })
                .ToListAsync();

            var groups = logs
                .GroupBy(e => new { e.Message, e.Severity })
                .Select(g => new ErrorScannerItemDto
                {
                    Message = g.Key.Message,
                    Severity = g.Key.Severity,
                    Count = g.Count(),
                    LastOccurredAt = g.Max(x => x.Timestamp),
                    Samples = g.Select(x => x.StackTrace ?? x.Message).Where(s => !string.IsNullOrWhiteSpace(s)).Take(3).ToList()
                })
                .OrderByDescending(i => i.Count)
                .ToList();

            var title = gameId.HasValue
                ? (await _gameRepository.FirstOrDefaultAsync(gameId.Value))?.Title ?? string.Empty
                : string.Empty;

            return new ErrorScannerResultDto
            {
                GameId = gameId,
                GameTitle = title,
                StartTime = start,
                EndTime = end,
                TotalErrors = logs.Count,
                Items = groups
            };
        }

        public async Task<PlayerFitDto> GetPlayerFitAsync(Guid gameId)
        {
            var game = await _gameRepository.GetAll()
                .Include(g => g.GameCategories)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null)
            {
                throw new Abp.UI.UserFriendlyException("Game not found");
            }

            var today = Clock.Now.Date;
            var sessions = await _playSessionRepository.GetAll()
                .Where(s => s.GameId == gameId && !s.IsPlaytest && s.StartedAt >= today.AddDays(-60))
                .Select(s => new { s.UserId, s.AnonymousIdHash, s.StartedAt })
                .ToListAsync();

            var playerDays = sessions
                .Select(s => new
                {
                    Key = s.UserId.HasValue ? $"u:{s.UserId.Value}" : $"a:{s.AnonymousIdHash}",
                    Date = s.StartedAt.Date
                })
                .Where(x => !string.IsNullOrEmpty(x.Key))
                .Distinct()
                .ToList();

            var firstPlayByPlayer = playerDays
                .GroupBy(x => x.Key)
                .ToDictionary(g => g.Key, g => g.Min(x => x.Date));

            var playerDateSets = playerDays
                .GroupBy(x => x.Key)
                .ToDictionary(g => g.Key, g => new HashSet<DateTime>(g.Select(x => x.Date)));

            long retained1d = 0, retained7d = 0, retained30d = 0;
            foreach (var player in firstPlayByPlayer)
            {
                var dates = playerDateSets[player.Key];
                if (dates.Contains(player.Value.AddDays(1))) retained1d++;
                if (dates.Contains(player.Value.AddDays(7))) retained7d++;
                if (dates.Contains(player.Value.AddDays(30))) retained30d++;
            }

            var totalPlayers = firstPlayByPlayer.Count;
            var retention1d = totalPlayers > 0 ? (double)retained1d / totalPlayers : 0;
            var retention7d = totalPlayers > 0 ? (double)retained7d / totalPlayers : 0;
            var retention30d = totalPlayers > 0 ? (double)retained30d / totalPlayers : 0;

            var last30Days = playerDays.Where(x => x.Date >= today.AddDays(-29)).ToList();
            var mau = last30Days.Select(x => x.Key).Distinct().Count();
            var dauWindow = last30Days
                .GroupBy(x => x.Date)
                .Select(g => g.Select(x => x.Key).Distinct().Count())
                .ToList();
            var avgDau = dauWindow.Any() ? dauWindow.Average() : 0.0;
            var stickiness = mau > 0 ? avgDau / mau : 0.0;

            var categoryId = game.GameCategories.Select(c => c.CategoryId).FirstOrDefault();
            var categoryAverageStickiness = categoryId != default
                ? await ComputeCategoryStickinessAsync(categoryId, gameId)
                : 0.0;

            var benchmark = stickiness >= categoryAverageStickiness * 1.1 ? "Above average"
                : stickiness >= categoryAverageStickiness * 0.9 ? "On par"
                : "Below average";

            var suggestions = new List<string>();
            if (retention1d < 0.3)
            {
                suggestions.Add("Primeira sessão curta; considere um tutorial mais envolvente ou recompensa diária.");
            }
            if (stickiness < 0.1)
            {
                suggestions.Add("Baixa recorrência; adicione notificações de retorno ou missões diárias.");
            }

            return new PlayerFitDto
            {
                GameId = gameId,
                GameTitle = game.Title,
                Retention1d = retention1d,
                Retention7d = retention7d,
                Retention30d = retention30d,
                Stickiness = stickiness,
                CategoryAverageStickiness = categoryAverageStickiness,
                Benchmark = benchmark,
                Suggestions = suggestions
            };
        }

        private async Task<double> ComputeCategoryStickinessAsync(Guid categoryId, Guid excludeGameId)
        {
            var gameIds = await _gameRepository.GetAll()
                .Where(g => g.Id != excludeGameId && g.GameCategories.Any(c => c.CategoryId == categoryId))
                .Select(g => g.Id)
                .ToListAsync();

            if (!gameIds.Any())
            {
                return 0.0;
            }

            var today = Clock.Now.Date;
            var sessions = await _playSessionRepository.GetAll()
                .Where(s => gameIds.Contains(s.GameId) && !s.IsPlaytest && s.StartedAt >= today.AddDays(-29))
                .Select(s => new { s.UserId, s.AnonymousIdHash, s.StartedAt })
                .ToListAsync();

            var playerDays = sessions
                .Select(s => new { Key = s.UserId.HasValue ? $"u:{s.UserId.Value}" : $"a:{s.AnonymousIdHash}", Date = s.StartedAt.Date })
                .Where(x => !string.IsNullOrEmpty(x.Key))
                .Distinct()
                .ToList();

            var mau = playerDays.Select(x => x.Key).Distinct().Count();
            var dau = playerDays
                .GroupBy(x => x.Date)
                .Select(g => g.Select(x => x.Key).Distinct().Count())
                .ToList();

            return mau > 0 ? dau.Average() / mau : 0.0;
        }

        public async Task<ConversionFunnelDto> GetConversionFunnelAsync(Guid? gameId, DateTime? startDate, DateTime? endDate)
        {
            var end = endDate?.Date ?? Clock.Now.Date;
            var start = startDate?.Date ?? end.AddDays(-29);
            var startAt = start;
            var endAt = end.AddDays(1).AddTicks(-1);

            var query = _metricSnapshotRepository.GetAll()
                .Where(s => s.Date >= startAt && s.Date <= endAt);

            if (gameId.HasValue)
            {
                query = query.Where(s => s.GameId == gameId.Value);
            }

            var snapshots = await query.ToListAsync();

            var pageViews = snapshots.Sum(s => s.PageViews);
            var loadingStarted = snapshots.Sum(s => s.LoadingStartedCount);
            var loadingFinished = snapshots.Sum(s => s.LoadingFinishedCount);
            var gameplayStarted = snapshots.Sum(s => s.GameplayStartedCount);

            var stages = new List<FunnelStageDto>
            {
                new() { Name = "PageView", Count = pageViews, ConversionRate = 1.0 },
                new() { Name = "LoadingStarted", Count = loadingStarted, ConversionRate = pageViews > 0 ? (double)loadingStarted / pageViews : 0 },
                new() { Name = "LoadingFinished", Count = loadingFinished, ConversionRate = loadingStarted > 0 ? (double)loadingFinished / loadingStarted : 0 },
                new() { Name = "GameplayStarted", Count = gameplayStarted, ConversionRate = loadingFinished > 0 ? (double)gameplayStarted / loadingFinished : 0 }
            };

            string title = string.Empty;
            if (gameId.HasValue)
            {
                title = (await _gameRepository.FirstOrDefaultAsync(gameId.Value))?.Title ?? string.Empty;
            }

            return new ConversionFunnelDto
            {
                GameId = gameId ?? Guid.Empty,
                GameTitle = title,
                StartDate = start,
                EndDate = end,
                Stages = stages
            };
        }

        private static double ComputeMedian(List<double> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0.0;
            }

            var sorted = values.OrderBy(v => v).ToList();
            var mid = sorted.Count / 2;
            return sorted.Count % 2 == 0
                ? (sorted[mid - 1] + sorted[mid]) / 2.0
                : sorted[mid];
        }

        private static List<MetricFpsDistributionItemDto> BuildFpsByDevice(IEnumerable<dynamic> sessions)
        {
            var groups = sessions
                .Where(s => s.FpsAverage != null)
                .GroupBy(s => string.IsNullOrWhiteSpace(s.DeviceType) ? "Unknown" : s.DeviceType)
                .Select(g =>
                {
                    var total = g.Count();
                    var acceptable = g.Count(s => s.FpsAverage >= 30);
                    return new MetricFpsDistributionItemDto
                    {
                        Device = g.Key,
                        TotalSessions = total,
                        AcceptableSessions = acceptable,
                        AcceptablePercentage = total == 0 ? 0.0 : (double)acceptable / total
                    };
                })
                .ToList();

            return groups;
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
