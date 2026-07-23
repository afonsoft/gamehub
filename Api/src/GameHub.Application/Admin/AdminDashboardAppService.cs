using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Eaf.Middleware.Authorization.Users;
using GameHub.Admin.Dto;
using GameHub.Authorization;
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
        private readonly IRepository<ModerationReview, Guid> _reviewRepository;
        private readonly IRepository<PlaySession, Guid> _playSessionRepository;
        private readonly IRepository<GameMetricSnapshot, Guid> _metricSnapshotRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<DeveloperProfile, Guid> _developerProfileRepository;

        public AdminDashboardAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<ModerationReview, Guid> reviewRepository,
            IRepository<PlaySession, Guid> playSessionRepository,
            IRepository<GameMetricSnapshot, Guid> metricSnapshotRepository,
            IRepository<User, long> userRepository,
            IRepository<DeveloperProfile, Guid> developerProfileRepository)
        {
            _gameRepository = gameRepository;
            _reviewRepository = reviewRepository;
            _playSessionRepository = playSessionRepository;
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

            return new AdminDashboardSummaryDto
            {
                TotalGames = totalGames,
                PendingReviews = pendingReviews,
                TotalPlays = totalPlays,
                ActiveUsers7d = activeUsers,
                TotalUsers = totalUsers,
                TotalDevelopers = totalDevelopers,
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
    }
}
