using System;
using System.Threading;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using GameHub.Catalog.Dto;
using Microsoft.Extensions.Caching.Memory;

namespace GameHub.Catalog
{
    public class InMemoryGameCatalogCache : IGameCatalogCache
    {
        private const string HomeCacheKey = "gamehub:catalog:home";
        private const string SlugCacheKeyPrefix = "gamehub:catalog:detail";
        private const string SearchCacheKeyPrefix = "gamehub:catalog:search";
        private const string CategoriesCacheKey = "gamehub:catalog:categories";
        private const string TagsCacheKey = "gamehub:catalog:tags";

        private readonly IMemoryCache _cache;
        private readonly IAbpSession _abpSession;

        public InMemoryGameCatalogCache(IMemoryCache cache, IAbpSession abpSession)
        {
            _cache = cache;
            _abpSession = abpSession;
        }

        public Task<HomeResponseDto> GetHomeAsync(CancellationToken cancellationToken = default)
        {
            _cache.TryGetValue(BuildKey(HomeCacheKey), out HomeResponseDto value);
            return Task.FromResult(value);
        }

        public Task SetHomeAsync(HomeResponseDto dto, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            _cache.Set(BuildKey(HomeCacheKey), dto, ttl);
            return Task.CompletedTask;
        }

        public Task InvalidateHomeAsync(CancellationToken cancellationToken = default)
        {
            _cache.Remove(BuildKey(HomeCacheKey));
            return Task.CompletedTask;
        }

        public Task<GameDetailDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            _cache.TryGetValue(BuildKey(SlugCacheKeyPrefix, slug), out GameDetailDto value);
            return Task.FromResult(value);
        }

        public Task SetBySlugAsync(string slug, GameDetailDto dto, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            _cache.Set(BuildKey(SlugCacheKeyPrefix, slug), dto, ttl);
            return Task.CompletedTask;
        }

        public Task InvalidateBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            _cache.Remove(BuildKey(SlugCacheKeyPrefix, slug));
            return Task.CompletedTask;
        }

        public Task<SearchResultDto> GetSearchAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            _cache.TryGetValue(BuildKey(SearchCacheKeyPrefix, cacheKey), out SearchResultDto value);
            return Task.FromResult(value);
        }

        public Task SetSearchAsync(string cacheKey, SearchResultDto dto, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            _cache.Set(BuildKey(SearchCacheKeyPrefix, cacheKey), dto, ttl);
            return Task.CompletedTask;
        }

        public Task InvalidateSearchAsync(CancellationToken cancellationToken = default)
        {
            // MemoryCache does not support prefix removal; rely on TTL for search entries.
            return Task.CompletedTask;
        }

        public Task<ListResultDto<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        {
            _cache.TryGetValue(BuildKey(CategoriesCacheKey), out ListResultDto<CategoryDto> value);
            return Task.FromResult(value);
        }

        public Task SetCategoriesAsync(ListResultDto<CategoryDto> dto, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            _cache.Set(BuildKey(CategoriesCacheKey), dto, ttl);
            return Task.CompletedTask;
        }

        public Task InvalidateCategoriesAsync(CancellationToken cancellationToken = default)
        {
            _cache.Remove(BuildKey(CategoriesCacheKey));
            return Task.CompletedTask;
        }

        public Task<ListResultDto<TagDto>> GetTagsAsync(CancellationToken cancellationToken = default)
        {
            _cache.TryGetValue(BuildKey(TagsCacheKey), out ListResultDto<TagDto> value);
            return Task.FromResult(value);
        }

        public Task SetTagsAsync(ListResultDto<TagDto> dto, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            _cache.Set(BuildKey(TagsCacheKey), dto, ttl);
            return Task.CompletedTask;
        }

        public Task InvalidateTagsAsync(CancellationToken cancellationToken = default)
        {
            _cache.Remove(BuildKey(TagsCacheKey));
            return Task.CompletedTask;
        }

        private string BuildKey(string prefix, string suffix = null)
        {
            var tenantId = _abpSession.TenantId?.ToString() ?? "host";
            return suffix == null ? $"{prefix}:{tenantId}" : $"{prefix}:{tenantId}:{suffix}";
        }
    }
}
