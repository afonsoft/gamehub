using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using GameHub.Authorization;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Developer.Dto;
using GameHub.Developers;
using GameHub.Gameplay;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Developer
{
    /// <summary>
    /// Expõe o dashboard do desenvolvedor com jogos, builds e ações pendentes.
    /// </summary>
    [AbpAuthorize(GameHubPermissions.Pages_Developer_Games)]
    public class DeveloperDashboardAppService : GameHubAppServiceBase, IDeveloperDashboardAppService
    {
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<GameBuild, Guid> _gameBuildRepository;
        private readonly IRepository<DeveloperProfile, Guid> _developerProfileRepository;
        private readonly IRepository<GameMetricSnapshot, Guid> _metricSnapshotRepository;
        private readonly IRepository<DeveloperTeamMember, Guid> _teamMemberRepository;

        public DeveloperDashboardAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<GameBuild, Guid> gameBuildRepository,
            IRepository<DeveloperProfile, Guid> developerProfileRepository,
            IRepository<GameMetricSnapshot, Guid> metricSnapshotRepository,
            IRepository<DeveloperTeamMember, Guid> teamMemberRepository)
        {
            _gameRepository = gameRepository;
            _gameBuildRepository = gameBuildRepository;
            _developerProfileRepository = developerProfileRepository;
            _metricSnapshotRepository = metricSnapshotRepository;
            _teamMemberRepository = teamMemberRepository;
        }

        public async Task<DeveloperDashboardDto> GetDashboardAsync()
        {
            await EnsureCurrentUserIsNotSupportAsync();

            if (!AbpSession.UserId.HasValue)
            {
                throw new AbpAuthorizationException("User is not authenticated.");
            }

            var profile = await _developerProfileRepository.FirstOrDefaultAsync(p => p.UserId == AbpSession.UserId.Value);
            if (profile == null)
            {
                return new DeveloperDashboardDto();
            }

            var games = await _gameRepository.GetAll()
                .Where(g => g.DeveloperProfileId == profile.Id && !g.IsDeleted)
                .ToListAsync();

            var gameIds = games.Select(g => g.Id).ToList();
            var recentBuilds = await _gameBuildRepository.GetAll()
                .Where(b => gameIds.Contains(b.GameId) && !b.IsDeleted)
                .OrderByDescending(b => b.CreationTime)
                .Take(10)
                .Include(b => b.Game)
                .ToListAsync();

            var playsOverTime = await BuildPlaysOverTimeAsync(gameIds);

            var gameById = games.ToDictionary(g => g.Id);

            return new DeveloperDashboardDto
            {
                TotalGames = games.Count,
                PublishedGames = games.Count(g => g.Status == GameStatus.Published),
                PendingReviewGames = games.Count(g => g.Status == GameStatus.InReview),
                DraftGames = games.Count(g => g.Status == GameStatus.Draft),
                RejectedGames = games.Count(g => g.Status == GameStatus.Rejected),
                TotalPlays = games.Sum(g => g.TotalPlays),
                RecentVersions = recentBuilds.Select(b => new DeveloperGameVersionDto
                {
                    Id = b.Id,
                    GameId = b.GameId,
                    GameTitle = b.Game?.Title ?? gameById.GetValueOrDefault(b.GameId)?.Title ?? string.Empty,
                    Version = b.Version,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreationTime,
                    PublishedAt = b.PublishedTime
                }).ToList(),
                PendingActions = games
                    .Where(g => g.Status == GameStatus.Draft || g.Status == GameStatus.Rejected || g.Status == GameStatus.InReview)
                    .Select(g => new DeveloperDashboardActionDto
                    {
                        GameId = g.Id,
                        Title = g.Title,
                        Slug = g.Slug,
                        Status = g.Status.ToString(),
                        Action = GetSuggestedAction(g.Status)
                    })
                    .ToList(),
                PlaysOverTime = playsOverTime
            };
        }

        private async Task<List<DeveloperDashboardDailyPlaysDto>> BuildPlaysOverTimeAsync(List<Guid> gameIds)
        {
            var end = Clock.Now.Date;
            var start = end.AddDays(-6);

            var snapshots = await _metricSnapshotRepository.GetAll()
                .Where(s => gameIds.Contains(s.GameId) && s.Date >= start && s.Date <= end)
                .ToListAsync();

            var playsByDate = snapshots
                .GroupBy(s => s.Date.Date)
                .ToDictionary(g => g.Key, g => g.Sum(s => s.Plays));

            var result = new List<DeveloperDashboardDailyPlaysDto>();
            for (var day = start; day <= end; day = day.AddDays(1))
            {
                result.Add(new DeveloperDashboardDailyPlaysDto
                {
                    Date = day,
                    Plays = playsByDate.GetValueOrDefault(day)
                });
            }

            return result;
        }

        private static string GetSuggestedAction(GameStatus status)
        {
            return status switch
            {
                GameStatus.Draft => "Finalize e submeta para revisão",
                GameStatus.InReview => "Aguarde a moderação",
                GameStatus.Rejected => "Corrija os problemas e reenvie",
                _ => string.Empty
            };
        }

        private async Task EnsureCurrentUserIsNotSupportAsync()
        {
            if (!AbpSession.UserId.HasValue)
            {
                return;
            }

            var member = await _teamMemberRepository.FirstOrDefaultAsync(m => m.UserId == AbpSession.UserId.Value);
            if (member?.Role == DeveloperTeamRole.Support)
            {
                throw new AbpAuthorizationException("Support team members cannot access the developer dashboard.");
            }
        }
    }
}
