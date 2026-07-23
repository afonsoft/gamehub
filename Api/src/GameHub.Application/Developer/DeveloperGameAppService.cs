using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using GameHub;
using GameHub.Authorization;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Catalog.Dto;
using GameHub.Developer.Dto;
using GameHub.Developers;
using GameHub.Moderation;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Developer
{
    public class DeveloperGameAppService : GameHubAppServiceBase, IDeveloperGameAppService
    {
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<GameBuild, Guid> _gameBuildRepository;
        private readonly IRepository<ModerationReview, Guid> _moderationReviewRepository;
        private readonly IRepository<DeveloperProfile, Guid> _developerProfileRepository;
        private readonly IGameCatalogCache _catalogCache;

        public DeveloperGameAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<GameBuild, Guid> gameBuildRepository,
            IRepository<ModerationReview, Guid> moderationReviewRepository,
            IRepository<DeveloperProfile, Guid> developerProfileRepository,
            IGameCatalogCache catalogCache)
        {
            _gameRepository = gameRepository;
            _gameBuildRepository = gameBuildRepository;
            _moderationReviewRepository = moderationReviewRepository;
            _developerProfileRepository = developerProfileRepository;
            _catalogCache = catalogCache;
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_Create)]
        public async Task<GameDetailDto> CreateDraftAsync(CreateGameDraftInput input)
        {
            var profile = await GetOrCreateProfileAsync();

            var id = Guid.NewGuid();
            var slug = await GetUniqueSlugAsync(Slug.Create(input.Title).Value);
            var game = new Game(
                id,
                input.Title,
                slug,
                input.ShortDescription,
                profile.Id);

            ObjectMapper.Map(input, game);
            game.TenantId = AbpSession.TenantId ?? profile.TenantId;
            game.SetCategories(input.CategoryIds ?? new List<Guid>());
            game.SetTags(input.TagIds ?? new List<Guid>());

            await _gameRepository.InsertAsync(game);
            await CurrentUnitOfWork.SaveChangesAsync();

            await _catalogCache.InvalidateHomeAsync();

            return ObjectMapper.Map<GameDetailDto>(game);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_Edit)]
        public async Task<GameDetailDto> UpdateMetadataAsync(UpdateGameMetadataInput input)
        {
            var game = await _gameRepository.GetAll()
                .Where(g => g.Id == input.GameId && !g.IsDeleted)
                .Include(g => g.GameCategories)
                .Include(g => g.GameTags)
                .FirstOrDefaultAsync();

            if (game == null)
            {
                throw new InvalidOperationException($"Game {input.GameId} not found.");
            }

            ObjectMapper.Map(input, game);

            if (input.CategoryIds != null)
            {
                game.SetCategories(input.CategoryIds);
            }

            if (input.TagIds != null)
            {
                game.SetTags(input.TagIds);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            await _catalogCache.InvalidateHomeAsync();

            return ObjectMapper.Map<GameDetailDto>(game);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_Edit)]
        public async Task<GameDetailDto> SubmitForReviewAsync(SubmitGameForReviewInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);

            if (game.Status != GameStatus.Draft && game.Status != GameStatus.Rejected)
            {
                throw new UserFriendlyException($"Game cannot be submitted for review from status {game.Status}.");
            }

            await EnsureCurrentUserOwnsGameAsync(game);

            var latestBuild = await _gameBuildRepository.GetAll()
                .Where(b => b.GameId == input.GameId && !b.IsDeleted)
                .OrderByDescending(b => b.BuildNumber)
                .FirstOrDefaultAsync();

            if (latestBuild == null)
            {
                throw new UserFriendlyException("Upload a build before submitting for review.");
            }

            if (latestBuild.Status != GameBuildStatus.Approved)
            {
                throw new UserFriendlyException("Approve the build in your developer panel before submitting for review.");
            }

            game.Status = GameStatus.InReview;

            var review = new ModerationReview
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                GameBuildId = latestBuild.Id,
                Status = ModerationReviewStatus.Pending,
                Notes = input.Notes ?? string.Empty,
            };

            await _moderationReviewRepository.InsertAsync(review);
            await CurrentUnitOfWork.SaveChangesAsync();
            await _catalogCache.InvalidateHomeAsync();

            return ObjectMapper.Map<GameDetailDto>(game);
        }

        [AbpAuthorize]
        public async Task<BuildDto> ApproveBuildAsync(DeveloperApproveBuildInput input)
        {
            var build = await _gameBuildRepository.GetAsync(input.GameBuildId);
            var game = await _gameRepository.GetAsync(build.GameId);

            await EnsureCurrentUserOwnsGameAsync(game);

            build.Approve();
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<BuildDto>(build);
        }

        [AbpAuthorize]
        public async Task<BuildDto> RejectBuildAsync(DeveloperRejectBuildInput input)
        {
            var build = await _gameBuildRepository.GetAsync(input.GameBuildId);
            var game = await _gameRepository.GetAsync(build.GameId);

            await EnsureCurrentUserOwnsGameAsync(game);

            build.Reject(input.Reason);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<BuildDto>(build);
        }

        [AbpAuthorize]
        public async Task<PagedResultDto<GameSummaryDto>> GetMyGamesAsync(GetGamesInput input)
        {
            var profile = await GetOrCreateProfileAsync();

            var query = _gameRepository.GetAll()
                .Where(g => g.DeveloperProfileId == profile.Id && !g.IsDeleted)
                .Include(g => g.PublishedBuild);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(g => g.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return new PagedResultDto<GameSummaryDto>(total, ObjectMapper.Map<List<GameSummaryDto>>(items));
        }

        [AbpAuthorize]
        public async Task<ListResultDto<BuildDto>> GetBuildsAsync(Guid gameId)
        {
            var game = await _gameRepository.GetAll()
                .Where(g => g.Id == gameId && !g.IsDeleted)
                .Include(g => g.GameBuilds)
                .FirstAsync();

            return new ListResultDto<BuildDto>(ObjectMapper.Map<List<BuildDto>>(game.GameBuilds.ToList()));
        }

        private async Task EnsureCurrentUserOwnsGameAsync(Game game)
        {
            var profile = await _developerProfileRepository.GetAsync(game.DeveloperProfileId);
            if (profile.UserId != AbpSession.UserId)
            {
                throw new AbpAuthorizationException("You can only manage builds of your own games.");
            }
        }

        private async Task<DeveloperProfile> GetOrCreateProfileAsync()
        {
            var userId = AbpSession.UserId ?? 0;
            var profile = await _developerProfileRepository.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile != null)
            {
                return profile;
            }

            var user = await UserManager.FindByIdAsync(userId.ToString());
            var displayName = $"{user?.Name} {user?.Surname}".Trim();
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = user?.UserName ?? "anonymous";
            }

            profile = new DeveloperProfile
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId ?? user?.TenantId,
                UserId = userId,
                DisplayName = displayName,
                Status = DeveloperProfileStatus.Pending
            };

            await _developerProfileRepository.InsertAsync(profile);
            return profile;
        }

        private async Task<string> GetUniqueSlugAsync(string baseSlug)
        {
            if (!await _gameRepository.GetAll().AnyAsync(g => g.Slug == baseSlug && !g.IsDeleted))
            {
                return baseSlug;
            }

            var suffix = 2;
            while (true)
            {
                var candidate = $"{baseSlug}-{suffix}";
                if (!await _gameRepository.GetAll().AnyAsync(g => g.Slug == candidate && !g.IsDeleted))
                {
                    return candidate;
                }

                suffix++;
            }
        }
    }
}
