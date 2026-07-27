using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GameHub.Web.HealthChecks
{
    /// <summary>
    /// Verifies that the configured distributed cache (Redis or in-memory fallback)
    /// can read and write a synthetic key with a short TTL.
    /// </summary>
    public class RedisCacheHealthCheck : IHealthCheck
    {
        private readonly IDistributedCache _cache;

        public RedisCacheHealthCheck(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var key = $"health:cache:{Guid.NewGuid():N}";
            var value = Guid.NewGuid().ToString("N");

            try
            {
                await _cache.SetStringAsync(
                    key,
                    value,
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5) },
                    cancellationToken);

                var retrieved = await _cache.GetStringAsync(key, cancellationToken);
                await _cache.RemoveAsync(key, cancellationToken);

                if (retrieved != value)
                {
                    return HealthCheckResult.Unhealthy("Cache round-trip returned an unexpected value.");
                }

                return HealthCheckResult.Healthy("Cache round-trip succeeded.");
            }
            catch (Exception exception)
            {
                return HealthCheckResult.Unhealthy("Cache is unavailable.", exception);
            }
        }
    }
}
