using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using GameHub.Builds;
using GameHub.Catalog.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Catalog
{
    public class GameCatalogAppService : ApplicationService, IGameCatalogAppService
    {
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IGameCatalogCache _catalogCache;

        public GameCatalogAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<Category, Guid> categoryRepository,
            IGameCatalogCache catalogCache)
        {
            _gameRepository = gameRepository;
            _categoryRepository = categoryRepository;
            _catalogCache = catalogCache;
        }

        public async Task<HomeResponseDto> GetHomeAsync()
        {
            var cached = await _catalogCache.GetHomeAsync();
            if (cached != null)
            {
                return cached;
            }

            var publishedGames = await _gameRepository.GetAll()
                .Where(g => g.Status == GameStatus.Published && !g.IsDeleted)
                .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                .Include(g => g.DeveloperProfile)
                .Include(g => g.PublishedBuild)
                .OrderByDescending(g => g.TotalPlays)
                .ToListAsync();

            var highlights = publishedGames
                .Where(g => g.GamePlacements.Any(p => p.PlacementType == GamePlacementType.Featured && p.IsActive))
                .Select(MapToCard)
                .ToList();

            if (!highlights.Any())
            {
                highlights = publishedGames.Take(6).Select(MapToCard).ToList();
            }

            var newGames = publishedGames
                .OrderByDescending(g => g.CreationTime)
                .Take(12)
                .Select(MapToCard)
                .ToList();

            var mostPlayed = publishedGames
                .OrderByDescending(g => g.TotalPlays)
                .Take(12)
                .Select(MapToCard)
                .ToList();

            var trending = publishedGames
                .OrderByDescending(g => g.TotalPlays)
                .Take(12)
                .Select(MapToCard)
                .ToList();

            var categories = await _categoryRepository.GetAll()
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Slug = c.Slug, SortOrder = c.SortOrder })
                .ToListAsync();

            var result = new HomeResponseDto
            {
                Highlights = highlights,
                NewGames = newGames,
                MostPlayed = mostPlayed,
                Trending = trending,
                Categories = categories
            };

            await _catalogCache.SetHomeAsync(result, TimeSpan.FromMinutes(5));
            return result;
        }

        public async Task<PagedResultDto<GameCardDto>> GetGamesAsync(GetGamesInput input)
        {
            System.Linq.IQueryable<Game> query = _gameRepository.GetAll()
                .Where(g => g.Status == GameStatus.Published && !g.IsDeleted)
                .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                .Include(g => g.DeveloperProfile)
                .Include(g => g.PublishedBuild);

            if (!string.IsNullOrWhiteSpace(input.CategorySlug))
            {
                query = query.Where(g => g.GameCategories.Any(gc => gc.Category.Slug == input.CategorySlug));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(g => g.TotalPlays)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return new PagedResultDto<GameCardDto>(total, items.Select(MapToCard).ToList());
        }

        public async Task<GameDetailDto> GetBySlugAsync(string slug)
        {
            var game = await _gameRepository.GetAll()
                .Where(g => g.Slug == slug && !g.IsDeleted)
                .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                .Include(g => g.GameTags)
                    .ThenInclude(gt => gt.Tag)
                .Include(g => g.DeveloperProfile)
                .Include(g => g.PublishedBuild)
                .FirstOrDefaultAsync();

            if (game == null)
            {
                return null;
            }

            return MapToDetail(game);
        }

        public async Task<SearchResultDto> SearchAsync(SearchInput input)
        {
            System.Linq.IQueryable<Game> query = _gameRepository.GetAll()
                .Where(g => g.Status == GameStatus.Published && !g.IsDeleted)
                .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                .Include(g => g.DeveloperProfile)
                .Include(g => g.PublishedBuild);

            if (!string.IsNullOrWhiteSpace(input.Query))
            {
                var q = input.Query.ToLowerInvariant();
                query = query.Where(g => g.Title.ToLower().Contains(q) || g.ShortDescription.ToLower().Contains(q));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(g => g.TotalPlays)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return new SearchResultDto
            {
                TotalCount = total,
                Items = items.Select(MapToCard).ToList()
            };
        }

        public async Task<ListResultDto<GameCardDto>> GetRelatedAsync(Guid gameId)
        {
            var game = await _gameRepository.GetAsync(gameId);
            var categoryIds = game.GameCategories.Select(gc => gc.CategoryId).ToList();

            var related = await _gameRepository.GetAll()
                .Where(g => g.Id != gameId && g.Status == GameStatus.Published && !g.IsDeleted)
                .Where(g => g.GameCategories.Any(gc => categoryIds.Contains(gc.CategoryId)))
                .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                .Include(g => g.DeveloperProfile)
                .Include(g => g.PublishedBuild)
                .Take(6)
                .ToListAsync();

            return new ListResultDto<GameCardDto>(related.Select(MapToCard).ToList());
        }

        private GameCardDto MapToCard(Game game)
        {
            return new GameCardDto
            {
                Id = game.Id,
                Title = game.Title,
                Slug = game.Slug,
                ShortDescription = game.ShortDescription,
                ThumbnailUrl = game.ThumbnailUrl,
                SupportsDesktop = game.SupportsDesktop,
                SupportsMobile = game.SupportsMobile,
                TotalPlays = game.TotalPlays,
                Categories = game.GameCategories
                    .Where(gc => gc.Category != null)
                    .Select(gc => new CategoryDto
                    {
                        Id = gc.Category.Id,
                        Name = gc.Category.Name,
                        Slug = gc.Category.Slug,
                        SortOrder = gc.Category.SortOrder
                    })
                    .ToList()
            };
        }

        private GameDetailDto MapToDetail(Game game)
        {
            return new GameDetailDto
            {
                Id = game.Id,
                Title = game.Title,
                Slug = game.Slug,
                ShortDescription = game.ShortDescription,
                Description = game.Description,
                Instructions = game.Instructions,
                AgeRating = game.AgeRating,
                Orientation = game.Orientation.ToString(),
                ThumbnailUrl = game.ThumbnailUrl,
                HeroImageUrl = game.HeroImageUrl,
                DeveloperName = game.DeveloperProfile?.DisplayName,
                PublishedBuildUrl = game.PublishedBuild != null
                    ? $"{game.PublishedBuild.PublicBaseUrl?.TrimEnd('/')}/{game.PublishedBuild.IndexHtmlPath?.TrimStart('/')}"
                    : null,
                TotalPlays = game.TotalPlays,
                AverageRating = (decimal)(game.AverageRating ?? 0),
                Tags = game.GameTags
                    .Where(gt => gt.Tag != null)
                    .Select(gt => new TagDto
                    {
                        Id = gt.Tag.Id,
                        Name = gt.Tag.Name,
                        Slug = gt.Tag.Slug
                    })
                    .ToList()
            };
        }
    }
}
