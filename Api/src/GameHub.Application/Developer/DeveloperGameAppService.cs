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
using GameHub.Builds.Dto;
using GameHub.Catalog;
using GameHub.Catalog.Dto;
using GameHub.Developer.Dto;
using GameHub.Developers;
using GameHub.Inspector;
using GameHub.Inspector.Dto;
using GameHub.Moderation;
using GameHub.Storage;
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
        private readonly IGameAssetStorage _assetStorage;
        private readonly IGamePreviewAppService _gamePreviewAppService;
        private readonly IInspectorAppService _inspectorAppService;

        private static readonly string[] AllowedImageExtensions = { ".png", ".jpg", ".jpeg", ".webp", ".gif" };

        public DeveloperGameAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<GameBuild, Guid> gameBuildRepository,
            IRepository<ModerationReview, Guid> moderationReviewRepository,
            IRepository<DeveloperProfile, Guid> developerProfileRepository,
            IGameCatalogCache catalogCache,
            IGameAssetStorage assetStorage,
            IGamePreviewAppService gamePreviewAppService,
            IInspectorAppService inspectorAppService)
        {
            _gameRepository = gameRepository;
            _gameBuildRepository = gameBuildRepository;
            _moderationReviewRepository = moderationReviewRepository;
            _developerProfileRepository = developerProfileRepository;
            _catalogCache = catalogCache;
            _assetStorage = assetStorage;
            _gamePreviewAppService = gamePreviewAppService;
            _inspectorAppService = inspectorAppService;
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
            await _catalogCache.InvalidateBySlugAsync(game.Slug);

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
            await _catalogCache.InvalidateBySlugAsync(game.Slug);

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

            var builds = ObjectMapper.Map<List<BuildDto>>(game.GameBuilds.ToList());
            foreach (var build in builds)
            {
                build.GameId = game.Id;
                build.GameSlug = game.Slug;
            }

            return new ListResultDto<BuildDto>(builds);
        }

        [AbpAuthorize]
        public async Task<ListResultDto<BuildDto>> GetVersionsAsync(Guid gameId)
        {
            return await GetBuildsAsync(gameId);
        }

        [AbpAuthorize]
        public async Task<List<DeveloperReviewHistoryItemDto>> GetReviewHistoryAsync(Guid gameId)
        {
            var game = await _gameRepository.GetAsync(gameId);
            await EnsureCurrentUserOwnsGameAsync(game);

            var reviews = await _moderationReviewRepository.GetAll()
                .Where(review => review.GameId == gameId && !review.IsDeleted)
                .OrderByDescending(review => review.CreationTime)
                .ToListAsync();

            return reviews.Select(review => new DeveloperReviewHistoryItemDto
            {
                Id = review.Id,
                GameId = review.GameId,
                GameBuildId = review.GameBuildId,
                Status = review.Status.ToString(),
                Decision = review.Decision?.ToString() ?? string.Empty,
                Notes = review.Notes ?? string.Empty,
                CreatedAt = review.CreationTime,
                CompletedAt = review.CompletedAt
            }).ToList();
        }

        [AbpAuthorize]
        public async Task<CreatePreviewTokenResult> CreatePreviewTokenForBuildAsync(CreatePreviewTokenInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);
            await EnsureCurrentUserOwnsGameAsync(game);
            return await _gamePreviewAppService.CreatePreviewTokenAsync(input);
        }

        [AbpAuthorize]
        public async Task<InspectorSessionDto> StartInspectorSessionForBuildAsync(StartInspectorSessionInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);
            await EnsureCurrentUserOwnsGameAsync(game);
            return await _inspectorAppService.StartSessionAsync(input);
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

        [AbpAuthorize(GameHubPermissions.Pages_Games_Edit)]
        public async Task<UploadImageResultDto> UploadThumbnailAsync(Guid gameId, byte[] fileBytes, string fileName, string contentType)
        {
            return await UploadImageAsync(gameId, fileBytes, fileName, contentType, "thumbnails", "static", (game, url) => game.SetThumbnail(url));
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_Edit)]
        public async Task<UploadImageResultDto> UploadHeroAsync(Guid gameId, byte[] fileBytes, string fileName, string contentType)
        {
            return await UploadImageAsync(gameId, fileBytes, fileName, contentType, "heroes", "hero", (game, url) => game.HeroImageUrl = url);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_Edit)]
        public async Task<UploadImageResultDto> UploadAnimatedThumbnailAsync(Guid gameId, byte[] fileBytes, string fileName, string contentType)
        {
            return await UploadImageAsync(gameId, fileBytes, fileName, contentType, "thumbnails", "animated", (game, url) => game.SetAnimatedThumbnail(url));
        }

        private async Task<UploadImageResultDto> UploadImageAsync(Guid gameId, byte[] fileBytes, string fileName, string contentType, string assetKind, string assetName, System.Action<Game, string> urlSetter)
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                throw new UserFriendlyException("Image file is required.");
            }

            var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
            if (System.Array.IndexOf(AllowedImageExtensions, extension) < 0)
            {
                throw new UserFriendlyException($"Image type not allowed. Allowed: {string.Join(", ", AllowedImageExtensions)}");
            }

            const long maxSize = 2L * 1024 * 1024;
            if (fileBytes.Length > maxSize)
            {
                throw new UserFriendlyException("Image must be 2 MB or smaller.");
            }

            var game = await _gameRepository.GetAsync(gameId);
            await EnsureCurrentUserOwnsGameAsync(game);

            using var stream = new System.IO.MemoryStream(fileBytes);
            var input = new AssetUploadInput
            {
                GameId = gameId,
                AssetKind = assetKind,
                FileName = $"{assetName}{extension}",
                ContentType = contentType,
                Content = stream,
            };

            var stored = await _assetStorage.StoreAssetAsync(input);

            urlSetter(game, stored.Url);

            await CurrentUnitOfWork.SaveChangesAsync();
            await _catalogCache.InvalidateBySlugAsync(game.Slug);

            return ObjectMapper.Map<UploadImageResultDto>(stored);
        }
    }
}
