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
using GameHub.Storage;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Admin
{
    public class AdminBuildAppService : GameHubAppServiceBase, IAdminBuildAppService
    {
        private readonly IRepository<GameBuild, Guid> _buildRepository;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IGameAssetStorage _assetStorage;

        public AdminBuildAppService(
            IRepository<GameBuild, Guid> buildRepository,
            IRepository<Game, Guid> gameRepository,
            IGameAssetStorage assetStorage)
        {
            _buildRepository = buildRepository;
            _gameRepository = gameRepository;
            _assetStorage = assetStorage;
        }

        [AbpAuthorize(GameHubPermissions.Pages_Builds_View)]
        public async Task<PagedResultDto<AdminBuildListItemDto>> GetAllBuildsAsync(GetBuildsInput input)
        {
            IQueryable<GameBuild> query = _buildRepository.GetAll()
                .Where(b => !b.IsDeleted)
                .Include(b => b.Game)
                    .ThenInclude(g => g.DeveloperProfile);

            if (!string.IsNullOrWhiteSpace(input.Status) && Enum.TryParse<GameBuildStatus>(input.Status, true, out var status))
            {
                query = query.Where(b => b.Status == status);
            }

            if (input.GameId.HasValue)
            {
                query = query.Where(b => b.GameId == input.GameId.Value);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(b => b.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var dtos = items.Select(MapToListItem).ToList();
            return new PagedResultDto<AdminBuildListItemDto>(total, dtos);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Builds_View)]
        public async Task<ListResultDto<BuildFileDto>> GetBuildFilesAsync(Guid buildId)
        {
            var build = await _buildRepository.GetAsync(buildId);
            var files = await _assetStorage.ListBuildFilesAsync(build.GameId, build.Id);

            var dtos = files.Select(f => new BuildFileDto
            {
                Name = f.Name,
                Key = f.Key,
                SizeBytes = f.SizeBytes,
                Url = f.Url,
                ContentType = f.ContentType,
                LastModified = f.LastModified,
                IsIndexHtml = string.Equals(f.Name, build.IndexHtmlPath, StringComparison.OrdinalIgnoreCase)
                    || f.Name.EndsWith("index.html", StringComparison.OrdinalIgnoreCase)
            }).ToList();

            return new ListResultDto<BuildFileDto>(dtos);
        }

        private static AdminBuildListItemDto MapToListItem(GameBuild build)
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
