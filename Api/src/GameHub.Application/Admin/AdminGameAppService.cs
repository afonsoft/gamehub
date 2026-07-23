using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using GameHub.Admin.Dto;
using GameHub.Authorization;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Catalog.Dto;
using GameHub.Developer.Dto;
using GameHub.Moderation;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Admin
{
    public class AdminGameAppService : GameHubAppServiceBase, IAdminGameAppService
    {
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<GameBuild, Guid> _buildRepository;
        private readonly IGameCatalogCache _catalogCache;

        public AdminGameAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<GameBuild, Guid> buildRepository,
            IGameCatalogCache catalogCache)
        {
            _gameRepository = gameRepository;
            _buildRepository = buildRepository;
            _catalogCache = catalogCache;
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_View)]
        public async Task<PagedResultDto<AdminGameListItemDto>> GetAllAsync(GetGamesInput input)
        {
            IQueryable<Game> query = _gameRepository.GetAll()
                .Where(g => !g.IsDeleted)
                .Include(g => g.DeveloperProfile);

            if (!string.IsNullOrWhiteSpace(input.Status) && Enum.TryParse<GameStatus>(input.Status, true, out var status))
            {
                query = query.Where(g => g.Status == status);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(g => g.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return new PagedResultDto<AdminGameListItemDto>(total, ObjectMapper.Map<List<AdminGameListItemDto>>(items));
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_View)]
        public async Task<AdminGameDetailDto> GetDetailAsync(Guid gameId)
        {
            var game = await _gameRepository.GetAll()
                .Where(g => g.Id == gameId && !g.IsDeleted)
                .Include(g => g.DeveloperProfile)
                .Include(g => g.PublishedBuild)
                .Include(g => g.GameBuilds)
                .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                .Include(g => g.GameTags)
                    .ThenInclude(gt => gt.Tag)
                .Include(g => g.ModerationReviews)
                .FirstOrDefaultAsync();

            if (game == null)
            {
                return null;
            }

            var dto = ObjectMapper.Map<AdminGameDetailDto>(game);
            return dto;
        }

        [AbpAuthorize(GameHubPermissions.Pages_Builds_Approve)]
        public async Task ApproveBuildAsync(ApproveBuildInput input)
        {
            var build = await _buildRepository.GetAsync(input.GameBuildId);
            build.Approve();

            await _buildRepository.UpdateAsync(build);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize(GameHubPermissions.Pages_Builds_Reject)]
        public async Task RejectBuildAsync(RejectBuildInput input)
        {
            var build = await _buildRepository.GetAsync(input.GameBuildId);
            build.Reject(input.Reason);

            await _buildRepository.UpdateAsync(build);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_Publish)]
        public async Task PublishAsync(PublishGameInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);
            var build = await _buildRepository.GetAsync(input.GameBuildId);

            build.Publish();
            game.SetPublishedBuild(build);

            await _catalogCache.InvalidateHomeAsync();
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_Suspend)]
        public async Task SuspendAsync(SuspendGameInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);
            game.Status = GameStatus.Suspended;
            await _catalogCache.InvalidateHomeAsync();
        }
    }
}
