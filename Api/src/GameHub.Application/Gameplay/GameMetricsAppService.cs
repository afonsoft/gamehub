using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using GameHub.Authorization;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Exceptions;
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

            if (!input.IsPlaytest)
            {
                sessionsQuery = sessionsQuery.Where(s => !s.IsPlaytest);
            }

            if (!string.IsNullOrWhiteSpace(input.CountryCode))
            {
                sessionsQuery = sessionsQuery.Where(s => s.CountryCode == input.CountryCode);
            }

            if (!string.IsNullOrWhiteSpace(input.DeviceType))
            {
                sessionsQuery = sessionsQuery.Where(s => s.DeviceType == input.DeviceType);
            }

            if (!string.IsNullOrWhiteSpace(input.TrafficSource) &&
                Enum.TryParse<GameHub.Monetization.TrafficSource>(input.TrafficSource, true, out var trafficSource))
            {
                sessionsQuery = sessionsQuery.Where(s => s.TrafficSource == trafficSource);
            }

            if (!string.IsNullOrWhiteSpace(input.UtmSource))
            {
                sessionsQuery = sessionsQuery.Where(s => s.UtmSource == input.UtmSource);
            }

            if (!string.IsNullOrWhiteSpace(input.UtmMedium))
            {
                sessionsQuery = sessionsQuery.Where(s => s.UtmMedium == input.UtmMedium);
            }

            if (!string.IsNullOrWhiteSpace(input.UtmCampaign))
            {
                sessionsQuery = sessionsQuery.Where(s => s.UtmCampaign == input.UtmCampaign);
            }

            var sessions = await sessionsQuery.ToListAsync();
            var sessionIds = sessions.Select(s => s.Id).ToList();

            var eventsQuery = _gameplayEventRepository.GetAll()
                .Where(e => e.GameId == gameId
                    && e.OccurredAt >= start
                    && e.OccurredAt < end
                    && sessionIds.Contains(e.PlaySessionId));

            if (input.BuildId.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.BuildId == input.BuildId.Value);
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
                GameplayStartedCount = events.Count(e => e.EventType == GameplayEventType.GameplayStarted),
                PageViewCount = events.Count(e => e.EventType == GameplayEventType.GamePageViewed),
                ConversionCount = events.Count(e => string.Equals(e.EventName, "conversion", StringComparison.OrdinalIgnoreCase)),
                ErrorCount = events.Count(e => e.EventType == GameplayEventType.GameErrorCaptured),
                CommercialBreakCount = events.Count(e => e.EventType == GameplayEventType.CommercialBreakCompleted),
                RewardedBreakCount = events.Count(e => e.EventType == GameplayEventType.RewardedBreakCompleted),
                Daily = daily
            };
        }

        public async Task<GameMetricsExportDto> ExportCsvAsync(Guid gameId, GameMetricsFilter input)
        {
            var result = await GetMetricsAsync(gameId, input);
            var csv = new StringBuilder();
            csv.AppendLine(
                "date,plays,uniquePlayers,avgDurationSeconds," +
                "loadingFinishedCount,gameplayStartedCount,pageViewCount," +
                "conversionCount,errorCount,commercialBreakCount,rewardedBreakCount");

            foreach (var item in result.Daily)
            {
                csv.Append(EscapeCsv(item.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
                csv.Append(',');
                csv.Append(item.Plays.ToString(CultureInfo.InvariantCulture));
                csv.Append(',');
                csv.Append(item.UniquePlayers.ToString(CultureInfo.InvariantCulture));
                csv.Append(',');
                csv.Append(item.AvgDurationSeconds.ToString(CultureInfo.InvariantCulture));
                csv.Append(',');
                csv.Append(item.LoadingFinishedCount.ToString(CultureInfo.InvariantCulture));
                csv.Append(',');
                csv.Append(item.GameplayStartedCount.ToString(CultureInfo.InvariantCulture));
                csv.Append(',');
                csv.Append(item.PageViewCount.ToString(CultureInfo.InvariantCulture));
                csv.Append(',');
                csv.Append(item.ConversionCount.ToString(CultureInfo.InvariantCulture));
                csv.Append(',');
                csv.Append(item.ErrorCount.ToString(CultureInfo.InvariantCulture));
                csv.Append(',');
                csv.Append(item.CommercialBreakCount.ToString(CultureInfo.InvariantCulture));
                csv.Append(',');
                csv.AppendLine(item.RewardedBreakCount.ToString(CultureInfo.InvariantCulture));
            }

            return new GameMetricsExportDto
            {
                FileName = $"metrics-{gameId:N}-{Clock.Now:yyyyMMdd}.csv",
                ContentType = "text/csv",
                Content = csv.ToString()
            };
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Contains('"'))
            {
                value = value.Replace("\"", "\"\"");
            }

            if (value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            {
                value = $"\"{value}\"";
            }

            return value;
        }

        private static void ValidateDateRange(GameMetricsFilter input)
        {
            if (input.From.HasValue && input.To.HasValue && input.From.Value.Date > input.To.Value.Date)
            {
                throw new GameHubException(
                    GameHubErrorCodes.ValidationFailed,
                    "A data inicial não pode ser posterior à data final.",
                    retryable: false);
            }
        }

        private async Task EnsureGameAccessAsync(Guid gameId)
        {
            if (!AbpSession.UserId.HasValue)
            {
                throw new GameHubException(
                    GameHubErrorCodes.NotAuthenticated,
                    "Usuário não autenticado.",
                    retryable: false);
            }

            var game = await _gameRepository.GetAll()
                .Where(g => g.Id == gameId && !g.IsDeleted)
                .Include(g => g.DeveloperProfile)
                .FirstOrDefaultAsync();

            if (game == null)
            {
                throw new GameHubException(
                    GameHubErrorCodes.InvalidContext,
                    "Jogo não encontrado.",
                    retryable: false);
            }

            var isAdmin = PermissionChecker.IsGranted(GameHubPermissions.Pages_Dashboard_View);
            var isOwner = game.DeveloperProfile != null && game.DeveloperProfile.UserId == AbpSession.UserId.Value;

            if (!isOwner && !isAdmin)
            {
                throw new GameHubException(
                    GameHubErrorCodes.NotAuthorized,
                    "Você não tem acesso às métricas deste jogo.",
                    retryable: false);
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
                GameplayStartedCount = eventList.Count(e => e.EventType == GameplayEventType.GameplayStarted),
                PageViewCount = eventList.Count(e => e.EventType == GameplayEventType.GamePageViewed),
                ConversionCount = eventList.Count(e => string.Equals(e.EventName, "conversion", StringComparison.OrdinalIgnoreCase)),
                ErrorCount = eventList.Count(e => e.EventType == GameplayEventType.GameErrorCaptured),
                CommercialBreakCount = eventList.Count(e => e.EventType == GameplayEventType.CommercialBreakCompleted),
                RewardedBreakCount = eventList.Count(e => e.EventType == GameplayEventType.RewardedBreakCompleted)
            };
        }
    }
}
