using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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

        private readonly IDistributedCache _cache;
        private readonly IAbpSession _abpSession;

        public RedisGameCatalogCache(IDistributedCache cache, IAbpSession abpSession)
        {
            _cache = cache;
            _abpSession = abpSession;
        }

        public async Task<HomeResponseDto> GetHomeAsync(CancellationToken cancellationToken = default)
        {
            var json = await _cache.GetStringAsync(GetKey(), cancellationToken);

            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<HomeResponseDto>(json);
        }

        public async Task SetHomeAsync(HomeResponseDto dto, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(dto);

            await _cache.SetStringAsync(
                GetKey(),
                json,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
                cancellationToken);
        }

        public async Task InvalidateHomeAsync(CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(GetKey(), cancellationToken);
        }

        private string GetKey()
        {
            var tenantId = _abpSession.TenantId?.ToString() ?? "host";
            return $"{HomeCacheKeyPrefix}:{tenantId}";
        }
    }
}
