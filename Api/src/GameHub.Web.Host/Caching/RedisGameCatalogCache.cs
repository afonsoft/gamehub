using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using GameHub.Catalog;
using GameHub.Catalog.Dto;
using Microsoft.Extensions.Caching.Distributed;

namespace GameHub.Web.Caching
{
    /// <summary>
    /// Redis-backed implementation of <see cref="IGameCatalogCache"/>.
    /// Cache key is scoped per tenant to support GameHub multi-tenancy.
    /// </summary>
    public class RedisGameCatalogCache : IGameCatalogCache
    {
        private const string HomeCacheKeyPrefix = "gamehub:catalog:home";
        private const string SlugCacheKeyPrefix = "gamehub:catalog:detail";
        private const string SearchCacheKeyPrefix = "gamehub:catalog:search";
        private const string CategoriesCacheKey = "gamehub:catalog:categories";
        private const string TagsCacheKey = "gamehub:catalog:tags";

        private readonly IDistributedCache _cache;
        private readonly IAbpSession _abpSession;

        public RedisGameCatalogCache(IDistributedCache cache, IAbpSession abpSession)
        {
            _cache = cache;
            _abpSession = abpSession;
        }

        public async Task<HomeResponseDto> GetHomeAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<HomeResponseDto>(BuildKey(HomeCacheKeyPrefix), cancellationToken);
        }

        public async Task SetHomeAsync(HomeResponseDto dto, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            await SetAsync(BuildKey(HomeCacheKeyPrefix), dto, ttl, cancellationToken);
        }

        public async Task InvalidateHomeAsync(CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(BuildKey(HomeCacheKeyPrefix), cancellationToken);
        }

        public async Task<GameDetailDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await GetAsync<GameDetailDto>(BuildKey(SlugCacheKeyPrefix, slug), cancellationToken);
        }

        public async Task SetBySlugAsync(string slug, GameDetailDto dto, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            await SetAsync(BuildKey(SlugCacheKeyPrefix, slug), dto, ttl, cancellationToken);
        }

        public async Task InvalidateBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(BuildKey(SlugCacheKeyPrefix, slug), cancellationToken);
        }

        public async Task<SearchResultDto> GetSearchAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            return await GetAsync<SearchResultDto>(BuildKey(SearchCacheKeyPrefix, cacheKey), cancellationToken);
        }

        public async Task SetSearchAsync(string cacheKey, SearchResultDto dto, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            await SetAsync(BuildKey(SearchCacheKeyPrefix, cacheKey), dto, ttl, cancellationToken);
        }

        public async Task InvalidateSearchAsync(CancellationToken cancellationToken = default)
        {
            // Redis prefix scanning is discouraged; search entries expire via TTL.
            await Task.CompletedTask;
        }

        public async Task<ListResultDto<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<ListResultDto<CategoryDto>>(BuildKey(CategoriesCacheKey), cancellationToken);
        }

        public async Task SetCategoriesAsync(ListResultDto<CategoryDto> dto, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            await SetAsync(BuildKey(CategoriesCacheKey), dto, ttl, cancellationToken);
        }

        public async Task InvalidateCategoriesAsync(CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(BuildKey(CategoriesCacheKey), cancellationToken);
        }

        public async Task<ListResultDto<TagDto>> GetTagsAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<ListResultDto<TagDto>>(BuildKey(TagsCacheKey), cancellationToken);
        }

        public async Task SetTagsAsync(ListResultDto<TagDto> dto, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            await SetAsync(BuildKey(TagsCacheKey), dto, ttl, cancellationToken);
        }

        public async Task InvalidateTagsAsync(CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(BuildKey(TagsCacheKey), cancellationToken);
        }

        private async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken)
        {
            var json = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json);
        }

        private async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(value);

            await _cache.SetStringAsync(
                key,
                json,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
                cancellationToken);
        }

        private string BuildKey(string prefix, string suffix = null)
        {
            var tenantId = _abpSession.TenantId?.ToString() ?? "host";
            return suffix == null ? $"{prefix}:{tenantId}" : $"{prefix}:{tenantId}:{suffix}";
        }
    }
}
