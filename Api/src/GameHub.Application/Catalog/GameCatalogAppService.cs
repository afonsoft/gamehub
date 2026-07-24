using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using GameHub.Builds;
using GameHub.Catalog.Dto;
using GameHub.Monetization;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Catalog
{
    public class GameCatalogAppService : GameHubAppServiceBase, IGameCatalogAppService
    {
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IRepository<Tag, Guid> _tagRepository;
        private readonly IGameCatalogCache _catalogCache;
        private readonly IGameSearchEngine _searchEngine;
        private readonly ITrendingScoreCalculator _trendingScoreCalculator;
        private readonly IRepository<GameVote, Guid> _gameVoteRepository;

        public GameCatalogAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<Category, Guid> categoryRepository,
            IRepository<Tag, Guid> tagRepository,
            IGameCatalogCache catalogCache,
            IGameSearchEngine searchEngine,
            ITrendingScoreCalculator trendingScoreCalculator,
            IRepository<GameVote, Guid> gameVoteRepository)
        {
            _gameRepository = gameRepository;
            _categoryRepository = categoryRepository;
            _tagRepository = tagRepository;
            _catalogCache = catalogCache;
            _searchEngine = searchEngine;
            _trendingScoreCalculator = trendingScoreCalculator;
            _gameVoteRepository = gameVoteRepository;
        }

        public async Task<HomeResponseDto> GetHomeAsync(CancellationToken cancellationToken = default)
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
                .Include(g => g.GamePlacements)
                .Include(g => g.DeveloperProfile)
                .Include(g => g.PublishedBuild)
                .Include(g => g.RevenueContracts)
                .ToListAsync();

            var popularScores = await _trendingScoreCalculator.CalculateScoresAsync(7);
            var trendingScores = await _trendingScoreCalculator.CalculateGrowthScoresAsync(7);

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
                .OrderByDescending(g => trendingScores.GetValueOrDefault(g.Id, 0))
                .ThenByDescending(g => g.TotalPlays)
                .Take(12)
                .Select(MapToCard)
                .ToList();

            var popularThisWeek = publishedGames
                .OrderByDescending(g => popularScores.GetValueOrDefault(g.Id, 0))
                .ThenByDescending(g => g.TotalPlays)
                .Take(12)
                .Select(MapToCard)
                .ToList();

            var webExclusives = publishedGames
                .Where(IsWebExclusive)
                .OrderByDescending(g => g.TotalPlays)
                .Take(12)
                .Select(MapToCard)
                .ToList();

            var categories = await _categoryRepository.GetAll()
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    SortOrder = c.SortOrder,
                    Description = c.Description,
                    Keywords = c.Keywords
                })
                .ToListAsync();

            var result = new HomeResponseDto
            {
                Highlights = highlights,
                NewGames = newGames,
                MostPlayed = mostPlayed,
                Trending = trending,
                PopularThisWeek = popularThisWeek,
                TopFree = mostPlayed,
                WebExclusives = webExclusives,
                Categories = categories
            };

            await _catalogCache.SetHomeAsync(result, TimeSpan.FromMinutes(5));
            return result;
        }

        public async Task<PagedResultDto<GameCardDto>> GetGamesAsync(GetGamesInput input, CancellationToken cancellationToken = default)
        {
            System.Linq.IQueryable<Game> query = _gameRepository.GetAll()
                .Where(g => g.Status == GameStatus.Published && !g.IsDeleted)
                .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                .Include(g => g.DeveloperProfile)
                .Include(g => g.PublishedBuild)
                .Include(g => g.RevenueContracts);

            if (!string.IsNullOrWhiteSpace(input.CategorySlug))
            {
                var categoryId = await _categoryRepository.GetAll()
                    .Where(c => c.Slug == input.CategorySlug && !c.IsDeleted)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (categoryId != Guid.Empty)
                {
                    query = query.Where(g => g.GameCategories.Any(gc => gc.CategoryId == categoryId));
                }
            }

            if (input.MinRating > 0)
            {
                query = query.Where(g => g.AverageRating.HasValue && (decimal)g.AverageRating.Value >= input.MinRating);
            }

            var exclusivity = input.Exclusivity?.ToLowerInvariant();
            if (exclusivity == "webexclusive")
            {
                query = query.Where(g => g.RevenueContracts.Any(c => c.IsActive && c.ContractType == RevenueContractType.WebExclusive));
            }
            else if (exclusivity == "nonexclusive")
            {
                query = query.Where(g => g.RevenueContracts.Any(c => c.IsActive && c.ContractType == RevenueContractType.NonExclusive));
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await ApplySorting(query, input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<GameCardDto>(total, items.Select(MapToCard).ToList());
        }

        public async Task<CategoryDto> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.GetAll()
                .Where(c => c.Slug == slug && c.IsActive && !c.IsDeleted)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    SortOrder = c.SortOrder,
                    Description = c.Description,
                    Keywords = c.Keywords
                })
                .FirstOrDefaultAsync(cancellationToken);

            return category;
        }

        public async Task<GameDetailDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var cached = await _catalogCache.GetBySlugAsync(slug, cancellationToken);
            if (cached != null)
            {
                return cached;
            }

            var game = await _gameRepository.GetAll()
                .Where(g => g.Slug == slug && !g.IsDeleted)
                .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                .Include(g => g.GameTags)
                    .ThenInclude(gt => gt.Tag)
                .Include(g => g.DeveloperProfile)
                .Include(g => g.PublishedBuild)
                .Include(g => g.RevenueContracts)
                .FirstOrDefaultAsync(cancellationToken);

            if (game == null)
            {
                return null;
            }

            var detail = MapToDetail(game);
            detail.RelatedGames = (await GetRelatedAsync(game.Id, cancellationToken)).Items.ToList();
            await _catalogCache.SetBySlugAsync(slug, detail, TimeSpan.FromMinutes(10), cancellationToken);
            return detail;
        }

        public async Task<GameVoteResultDto> GetVoteAsync(Guid gameId, string deviceId = null)
        {
            var game = await _gameRepository.GetAsync(gameId);
            var userVote = await GetCurrentVoteTypeAsync(gameId, deviceId);

            return new GameVoteResultDto
            {
                GameId = gameId,
                TotalLikes = game.TotalLikes,
                TotalDislikes = game.TotalDislikes,
                UserVote = userVote
            };
        }

        public async Task<GameVoteResultDto> VoteAsync(GameVoteInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);

            if (game.Status != GameStatus.Published)
            {
                throw new InvalidOperationException("Cannot vote on an unpublished game.");
            }

            var userId = AbpSession.UserId;
            var existingVote = await FindExistingVoteAsync(input.GameId, userId, input.DeviceId);

            if (existingVote != null)
            {
                if (existingVote.VoteType == input.VoteType)
                {
                    return new GameVoteResultDto
                    {
                        GameId = input.GameId,
                        TotalLikes = game.TotalLikes,
                        TotalDislikes = game.TotalDislikes,
                        UserVote = existingVote.VoteType
                    };
                }

                if (existingVote.VoteType == GameVoteType.Like)
                {
                    game.TotalLikes = Math.Max(0, game.TotalLikes - 1);
                    game.TotalDislikes++;
                }
                else
                {
                    game.TotalDislikes = Math.Max(0, game.TotalDislikes - 1);
                    game.TotalLikes++;
                }

                existingVote.VoteType = input.VoteType;
            }
            else
            {
                var vote = new GameVote
                {
                    Id = Guid.NewGuid(),
                    TenantId = AbpSession.TenantId,
                    GameId = input.GameId,
                    DeviceId = input.DeviceId,
                    VoteType = input.VoteType
                };

                if (input.VoteType == GameVoteType.Like)
                {
                    game.TotalLikes++;
                }
                else
                {
                    game.TotalDislikes++;
                }

                await _gameVoteRepository.InsertAsync(vote);
            }

            game.RecalculateRating();
            await CurrentUnitOfWork.SaveChangesAsync();

            return new GameVoteResultDto
            {
                GameId = input.GameId,
                TotalLikes = game.TotalLikes,
                TotalDislikes = game.TotalDislikes,
                UserVote = input.VoteType
            };
        }

        public async Task<SearchResultDto> SearchAsync(SearchInput input, CancellationToken cancellationToken = default)
        {
            var cacheKey = ComputeSearchCacheKey(input);
            var cached = await _catalogCache.GetSearchAsync(cacheKey, cancellationToken);
            if (cached != null)
            {
                return cached;
            }

            System.Linq.IQueryable<Game> query = _gameRepository.GetAll()
                .Where(g => g.Status == GameStatus.Published && !g.IsDeleted)
                .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                .Include(g => g.GameTags)
                    .ThenInclude(gt => gt.Tag)
                .Include(g => g.DeveloperProfile)
                .Include(g => g.PublishedBuild)
                .Include(g => g.RevenueContracts);

            query = _searchEngine.ApplySearchFilter(query, input.Query);

            if (input.Categories != null && input.Categories.Any())
            {
                var categorySlugs = input.Categories.Select(c => c.ToLowerInvariant()).ToList();
                var categoryIds = await _categoryRepository.GetAll()
                    .Where(c => categorySlugs.Contains(c.Slug.ToLower()) && !c.IsDeleted)
                    .Select(c => c.Id)
                    .ToListAsync(cancellationToken);

                if (categoryIds.Any())
                {
                    query = query.Where(g => g.GameCategories.Any(gc => categoryIds.Contains(gc.CategoryId)));
                }
            }

            if (input.Tags != null && input.Tags.Any())
            {
                var tagSlugs = input.Tags.Select(t => t.ToLowerInvariant()).ToList();
                var tagIds = await _tagRepository.GetAll()
                    .Where(t => tagSlugs.Contains(t.Slug.ToLower()) && !t.IsDeleted)
                    .Select(t => t.Id)
                    .ToListAsync(cancellationToken);

                if (tagIds.Any())
                {
                    query = query.Where(g => g.GameTags.Any(gt => tagIds.Contains(gt.TagId)));
                }
            }

            if (!string.IsNullOrWhiteSpace(input.Device))
            {
                var device = input.Device.ToLowerInvariant();
                if (device == "desktop")
                {
                    query = query.Where(g => g.SupportsDesktop);
                }
                else if (device == "mobile")
                {
                    query = query.Where(g => g.SupportsMobile);
                }
                else if (device == "tablet")
                {
                    query = query.Where(g => g.SupportsTablet);
                }
            }

            if (!string.IsNullOrWhiteSpace(input.Orientation))
            {
                if (Enum.TryParse<GameOrientation>(input.Orientation, true, out var orientation))
                {
                    query = query.Where(g => g.Orientation == orientation || g.Orientation == GameOrientation.Both);
                }
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(g => g.TotalPlays)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync(cancellationToken);

            var result = new SearchResultDto
            {
                TotalCount = total,
                Items = items.Select(MapToCard).ToList()
            };

            await _catalogCache.SetSearchAsync(cacheKey, result, TimeSpan.FromMinutes(2), cancellationToken);
            return result;
        }

        public async Task<ListResultDto<GameCardDto>> GetRelatedAsync(Guid gameId, CancellationToken cancellationToken = default)
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
                .Include(g => g.RevenueContracts)
                .Take(6)
                .ToListAsync();

            return new ListResultDto<GameCardDto>(related.Select(MapToCard).ToList());
        }

        private IQueryable<Game> ApplySorting(IQueryable<Game> query, string sorting)
        {
            var sort = sorting?.ToLowerInvariant();

            return sort switch
            {
                "newest" => query.OrderByDescending(g => g.CreationTime),
                "mostplayed" => query.OrderByDescending(g => g.TotalPlays),
                "toprated" => query.OrderByDescending(g => g.AverageRating ?? 0),
                "title" => query.OrderBy(g => g.Title),
                _ => query.OrderByDescending(g => g.TotalPlays)
            };
        }

        private static string ComputeSearchCacheKey(SearchInput input)
        {
            var builder = new StringBuilder();
            builder.Append(input.Query?.ToLowerInvariant() ?? string.Empty).Append('|');

            if (input.Categories?.Any() == true)
            {
                builder.Append(string.Join(',', input.Categories.OrderBy(c => c))).Append('|');
            }

            if (input.Tags?.Any() == true)
            {
                builder.Append(string.Join(',', input.Tags.OrderBy(t => t))).Append('|');
            }

            builder
                .Append(input.Device).Append('|')
                .Append(input.Orientation).Append('|')
                .Append(input.SkipCount).Append('|')
                .Append(input.MaxResultCount);

            var raw = Encoding.UTF8.GetBytes(builder.ToString());
            var hash = SHA256.HashData(raw);
            return Convert.ToHexString(hash);
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
                AnimatedThumbnailUrl = game.ThumbnailStatus == GameThumbnailStatus.Approved ? game.AnimatedThumbnailUrl : string.Empty,
                ThumbnailStatus = game.ThumbnailStatus.ToString(),
                AspectRatio = game.AspectRatio.ToString(),
                SupportsDesktop = game.SupportsDesktop,
                SupportsMobile = game.SupportsMobile,
                SupportsCloudSaves = game.SupportsCloudSaves,
                TotalPlays = game.TotalPlays,
                TotalLikes = game.TotalLikes,
                TotalDislikes = game.TotalDislikes,
                AverageRating = (decimal)ComputeAverageRating(game),
                TotalVotes = ComputeTotalVotes(game),
                IsWebExclusive = IsWebExclusive(game),
                Categories = game.GameCategories
                    .Where(gc => gc.Category != null)
                    .Select(gc => new CategoryDto
                    {
                        Id = gc.Category.Id,
                        Name = gc.Category.Name,
                        Slug = gc.Category.Slug,
                        SortOrder = gc.Category.SortOrder,
                        Description = gc.Category.Description,
                        Keywords = gc.Category.Keywords
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
                Controls = game.Controls,
                AgeRating = game.AgeRating,
                Orientation = game.Orientation.ToString(),
                ThumbnailUrl = game.ThumbnailUrl,
                AnimatedThumbnailUrl = game.ThumbnailStatus == GameThumbnailStatus.Approved ? game.AnimatedThumbnailUrl : string.Empty,
                ThumbnailStatus = game.ThumbnailStatus.ToString(),
                AspectRatio = game.AspectRatio.ToString(),
                HeroImageUrl = game.HeroImageUrl,
                DeveloperName = game.DeveloperProfile?.DisplayName,
                PublishedBuildUrl = game.PublishedBuild != null
                    ? $"{game.PublishedBuild.PublicBaseUrl?.TrimEnd('/')}/{game.PublishedBuild.IndexHtmlPath?.TrimStart('/')}"
                    : null,
                TotalPlays = game.TotalPlays,
                TotalLikes = game.TotalLikes,
                TotalDislikes = game.TotalDislikes,
                AverageRating = (decimal)ComputeAverageRating(game),
                TotalVotes = ComputeTotalVotes(game),
                SupportsDesktop = game.SupportsDesktop,
                SupportsMobile = game.SupportsMobile,
                SupportsTablet = game.SupportsTablet,
                SupportsCloudSaves = game.SupportsCloudSaves,
                ControlScheme = game.ControlScheme.ToString(),
                CutscenesSkippable = game.CutscenesSkippable,
                DefaultLanguage = game.DefaultLanguage,
                SupportedLanguages = ParseSupportedLanguages(game.SupportedLanguages),
                Categories = game.GameCategories
                    .Where(gc => gc.Category != null)
                    .Select(gc => new CategoryDto
                    {
                        Id = gc.Category.Id,
                        Name = gc.Category.Name,
                        Slug = gc.Category.Slug,
                        SortOrder = gc.Category.SortOrder,
                        Description = gc.Category.Description,
                        Keywords = gc.Category.Keywords
                    })
                    .ToList(),
                Tags = game.GameTags
                    .Where(gt => gt.Tag != null)
                    .Select(gt => new TagDto
                    {
                        Id = gt.Tag.Id,
                        Name = gt.Tag.Name,
                        Slug = gt.Tag.Slug
                    })
                    .ToList(),
                IsWebExclusive = IsWebExclusive(game)
            };
        }

        private async Task<GameVote> FindExistingVoteAsync(Guid gameId, long? userId, string deviceId)
        {
            if (userId.HasValue)
            {
                return await _gameVoteRepository.FirstOrDefaultAsync(v => v.GameId == gameId && v.CreatorUserId == userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                return await _gameVoteRepository.FirstOrDefaultAsync(v => v.GameId == gameId && v.DeviceId == deviceId);
            }

            return null;
        }

        private async Task<GameVoteType?> GetCurrentVoteTypeAsync(Guid gameId, string deviceId)
        {
            var vote = await FindExistingVoteAsync(gameId, AbpSession.UserId, deviceId);
            return vote?.VoteType;
        }

        private static double ComputeAverageRating(Game game)
        {
            var totalVotes = ComputeTotalVotes(game);
            if (totalVotes == 0)
            {
                return game.AverageRating ?? 0;
            }

            if (game.AverageRating.HasValue && game.AverageRating.Value > 0)
            {
                return game.AverageRating.Value;
            }

            return (double)game.TotalLikes / totalVotes * 5;
        }

        private static long ComputeTotalVotes(Game game)
        {
            return game.TotalLikes + game.TotalDislikes;
        }

        private static bool IsWebExclusive(Game game)
        {
            return game.RevenueContracts?.Any(c => c.IsActive && c.ContractType == RevenueContractType.WebExclusive) == true;
        }

        private static List<string> ParseSupportedLanguages(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new List<string>();
            }

            return value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToList();
        }
    }
}
