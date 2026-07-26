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
        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IRepository<GameBuild, Guid> _buildRepository;
        private readonly IRepository<BuildValidationReport, Guid> _validationReportRepository;
        private readonly IGameCatalogCache _catalogCache;

        public AdminGameAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<Category, Guid> categoryRepository,
            IRepository<GameBuild, Guid> buildRepository,
            IRepository<BuildValidationReport, Guid> validationReportRepository,
            IGameCatalogCache catalogCache)
        {
            _gameRepository = gameRepository;
            _categoryRepository = categoryRepository;
            _buildRepository = buildRepository;
            _validationReportRepository = validationReportRepository;
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

        [AbpAuthorize(GameHubPermissions.Pages_Games_Publish)]
        public async Task PublishAsync(PublishGameInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);
            var build = await _buildRepository.GetAsync(input.GameBuildId);

            var latestReport = await _validationReportRepository.GetAll()
                .Where(r => r.GameBuildId == build.Id)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestReport != null && latestReport.HasExternalRequests && string.IsNullOrWhiteSpace(game.PrivacyPolicyUrl))
            {
                throw new InvalidOperationException("This build contains external requests. A privacy policy URL is required before publishing.");
            }

            build.Publish();
            game.SetPublishedBuild(build);

            await _catalogCache.InvalidateHomeAsync();
            await _catalogCache.InvalidateBySlugAsync(game.Slug);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_Suspend)]
        public async Task SuspendAsync(SuspendGameInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);
            game.Status = GameStatus.Suspended;
            await CurrentUnitOfWork.SaveChangesAsync();
            await _catalogCache.InvalidateHomeAsync();
            await _catalogCache.InvalidateBySlugAsync(game.Slug);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_Publish)]
        public async Task StartReviewAsync(StartReviewInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);
            if (game.Status != GameStatus.Submitted && game.Status != GameStatus.InReview)
            {
                throw new InvalidOperationException("Review can only be started for a submitted or in-review game.");
            }

            game.Status = GameStatus.InReview;
            await CurrentUnitOfWork.SaveChangesAsync();
            await _catalogCache.InvalidateBySlugAsync(game.Slug);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_Publish)]
        public async Task ApproveForPublishingAsync(ApproveForPublishingInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);
            game.Status = GameStatus.ApprovedForPublishing;

            if (input.GameBuildId.HasValue)
            {
                var build = await _buildRepository.GetAsync(input.GameBuildId.Value);
                var latestReport = await _validationReportRepository.GetAll()
                    .Where(r => r.GameBuildId == build.Id)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                if (latestReport != null && latestReport.HasExternalRequests && string.IsNullOrWhiteSpace(game.PrivacyPolicyUrl))
                {
                    throw new InvalidOperationException("This build contains external requests. A privacy policy URL is required before publishing.");
                }

                build.Publish();
                game.SetPublishedBuild(build);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            await _catalogCache.InvalidateHomeAsync();
            await _catalogCache.InvalidateBySlugAsync(game.Slug);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_Suspend)]
        public async Task RequestChangesAsync(RequestChangesInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);
            game.Status = GameStatus.Rejected;
            await CurrentUnitOfWork.SaveChangesAsync();
            await _catalogCache.InvalidateBySlugAsync(game.Slug);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_Publish)]
        public async Task ApproveThumbnailAsync(Guid gameId)
        {
            var game = await _gameRepository.GetAsync(gameId);
            game.ApproveThumbnail();
            await CurrentUnitOfWork.SaveChangesAsync();
            await _catalogCache.InvalidateBySlugAsync(game.Slug);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_Suspend)]
        public async Task RejectThumbnailAsync(Guid gameId)
        {
            var game = await _gameRepository.GetAsync(gameId);
            game.RejectThumbnail();
            await CurrentUnitOfWork.SaveChangesAsync();
            await _catalogCache.InvalidateBySlugAsync(game.Slug);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_View)]
        public async Task<List<CategoryDto>> SuggestCategoriesAsync(SuggestCategoriesInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);
            var assignedCategoryIds = await _gameRepository.GetAll()
                .Where(g => g.Id == input.GameId)
                .SelectMany(g => g.GameCategories)
                .Select(gc => gc.CategoryId)
                .ToListAsync();

            var allCategories = await _categoryRepository.GetAll()
                .Where(c => c.IsActive && !assignedCategoryIds.Contains(c.Id))
                .ToListAsync();

            var searchTerms = new List<string>
            {
                game.Title,
                game.Description,
                game.ShortDescription,
                game.SuggestedDescription
            }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .SelectMany(s => s.Split(new[] { ' ', ',', '.', '-', '_' }, StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t.ToLowerInvariant())
            .Distinct()
            .ToList();

            var scoredCategories = allCategories
                .Select(c =>
                {
                    var score = 0;
                    var keywords = string.IsNullOrWhiteSpace(c.Keywords)
                        ? new List<string>()
                        : c.Keywords.ToLowerInvariant().Split(',').Select(k => k.Trim()).ToList();

                    if (searchTerms.Contains(c.Name.ToLowerInvariant()))
                    {
                        score += 10;
                    }
                    if (searchTerms.Contains(c.Slug.ToLowerInvariant()))
                    {
                        score += 5;
                    }
                    if (!string.IsNullOrWhiteSpace(c.Description) && searchTerms.Any(t => c.Description.ToLowerInvariant().Contains(t)))
                    {
                        score += 2;
                    }
                    score += keywords.Count(k => searchTerms.Contains(k));

                    return new { Category = c, Score = score };
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Category.SortOrder)
                .Take(5)
                .ToList();

            return ObjectMapper.Map<List<CategoryDto>>(scoredCategories.Select(x => x.Category).ToList());
        }

        [AbpAuthorize(GameHubPermissions.Pages_Games_View)]
        public async Task<ValidateSeoResultDto> ValidateSeoAsync(ValidateSeoInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);
            var warnings = new List<string>();

            if (string.IsNullOrWhiteSpace(game.SuggestedDescription))
            {
                warnings.Add("SuggestedDescription is empty. Provide a detailed description for moderation.");
            }
            else if (game.SuggestedDescription.Length < 50)
            {
                warnings.Add("SuggestedDescription is too short. Aim for at least 50 characters.");
            }

            if (string.IsNullOrWhiteSpace(game.SeoDescription))
            {
                warnings.Add("SeoDescription is empty. Provide a concise SEO description.");
            }
            else
            {
                if (game.SeoDescription.Length < 50)
                {
                    warnings.Add("SeoDescription is too short. Aim for 50-160 characters.");
                }
                if (game.SeoDescription.Length > 160)
                {
                    warnings.Add("SeoDescription exceeds 160 characters and may be truncated by search engines.");
                }
            }

            return new ValidateSeoResultDto
            {
                IsValid = warnings.Count == 0,
                Warnings = warnings,
                SuggestedDescription = game.SuggestedDescription ?? string.Empty,
                SeoDescription = game.SeoDescription ?? string.Empty
            };
        }
    }
}
