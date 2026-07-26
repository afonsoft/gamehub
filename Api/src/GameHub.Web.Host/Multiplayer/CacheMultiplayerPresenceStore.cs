using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Abp.Runtime.Caching;
using GameHub.Multiplayer;
using GameHub.Monitoring;
using Microsoft.Extensions.Options;

namespace GameHub.Web.Multiplayer
{
    /// <summary>
    /// Stores multiplayer presence through the ABP cache provider.
    /// </summary>
    public class CacheMultiplayerPresenceStore : IMultiplayerPresenceStore
    {
        private const string CacheName = "GameHub.Multiplayer.Presence";
        private const string KeyPrefix = "gamehub:multiplayer:presence";

        private readonly ITypedCache<string, MultiplayerPresenceEntry> _cache;
        private readonly MultiplayerPresenceOptions _options;

        public CacheMultiplayerPresenceStore(
            ICacheManager cacheManager,
            IOptions<MultiplayerPresenceOptions> options)
        {
            _cache = cacheManager
                .GetCache(CacheName)
                .AsTyped<string, MultiplayerPresenceEntry>();
            _options = options.Value;
        }

        public Task RegisterAsync(MultiplayerPresenceEntry entry, TimeSpan ttl)
        {
            EnsureValidTtl(ttl);
            entry.LastSeenAt = DateTimeOffset.UtcNow;
            return MeasureAsync(
                () => _cache.SetAsync(
                BuildConnectionKey(entry.TenantId, entry.ConnectionId),
                entry,
                absoluteExpireTime: DateTimeOffset.UtcNow.Add(ttl)),
                () => GameHubMetrics.PresenceRegistered.Add(1));
        }

        public Task<MultiplayerPresenceEntry> GetAsync(
            int? tenantId,
            Guid matchId,
            string connectionId)
        {
            return GetForMatchAsync(tenantId, matchId, connectionId);
        }

        public async Task<MultiplayerPresenceEntry> GetByConnectionAsync(
            int? tenantId,
            string connectionId)
        {
            return await _cache.GetOrDefaultAsync(
                BuildConnectionKey(tenantId, connectionId));
        }

        public async Task RefreshAsync(
            int? tenantId,
            Guid matchId,
            string connectionId,
            TimeSpan ttl)
        {
            EnsureValidTtl(ttl);
            var entry = await GetByConnectionAsync(tenantId, connectionId);
            if (entry == null)
            {
                return;
            }

            entry.LastSeenAt = DateTimeOffset.UtcNow;
            await MeasureAsync(
                () => _cache.SetAsync(
                BuildConnectionKey(tenantId, connectionId),
                entry,
                absoluteExpireTime: DateTimeOffset.UtcNow.Add(ttl)),
                () => GameHubMetrics.PresenceRefreshed.Add(1));
        }

        public Task RemoveAsync(int? tenantId, Guid matchId, string connectionId)
        {
            return RemoveForMatchAsync(tenantId, matchId, connectionId);
        }

        public Task RemoveByConnectionAsync(int? tenantId, string connectionId)
        {
            return MeasureAsync(
                () => _cache.RemoveAsync(BuildConnectionKey(tenantId, connectionId)),
                () => GameHubMetrics.PresenceRemoved.Add(1));
        }

        private async Task<MultiplayerPresenceEntry> GetForMatchAsync(
            int? tenantId,
            Guid matchId,
            string connectionId)
        {
            var entry = await GetByConnectionAsync(tenantId, connectionId);
            return entry?.MatchId == matchId ? entry : null;
        }

        private async Task RemoveForMatchAsync(
            int? tenantId,
            Guid matchId,
            string connectionId)
        {
            var entry = await GetForMatchAsync(tenantId, matchId, connectionId);
            if (entry != null)
            {
                await RemoveByConnectionAsync(tenantId, connectionId);
            }
        }

        private static string BuildConnectionKey(int? tenantId, string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                throw new ArgumentException("Connection id is required.", nameof(connectionId));
            }

            var tenantKey = tenantId?.ToString() ?? "host";
            return $"{KeyPrefix}:{tenantKey}:connection:{connectionId}";
        }

        private void EnsureValidTtl(TimeSpan ttl)
        {
            if (ttl <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(ttl));
            }

            var maximumTtl = _options.EntryTtl > TimeSpan.Zero
                ? _options.EntryTtl
                : TimeSpan.FromHours(1);
            if (ttl > maximumTtl)
            {
                throw new ArgumentOutOfRangeException(nameof(ttl));
            }
        }

        private static async Task MeasureAsync(
            Func<Task> operation,
            Action onSuccess)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await operation();
                onSuccess();
            }
            catch
            {
                GameHubMetrics.PresenceCacheErrors.Add(1);
                throw;
            }
            finally
            {
                GameHubMetrics.PresenceOperationDurationMs.Record(stopwatch.Elapsed.TotalMilliseconds);
            }
        }
    }
}
