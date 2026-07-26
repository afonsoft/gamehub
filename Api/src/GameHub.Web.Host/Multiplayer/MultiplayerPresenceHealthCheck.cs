using System;
using System.Threading;
using System.Threading.Tasks;
using GameHub.Multiplayer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace GameHub.Web.Multiplayer
{
    /// <summary>
    /// Verifies that the configured multiplayer presence cache can round-trip a value.
    /// </summary>
    public class MultiplayerPresenceHealthCheck : IHealthCheck
    {
        private readonly IMultiplayerPresenceStore _presenceStore;
        private readonly MultiplayerPresenceOptions _options;

        public MultiplayerPresenceHealthCheck(
            IMultiplayerPresenceStore presenceStore,
            IOptions<MultiplayerPresenceOptions> options)
        {
            _presenceStore = presenceStore;
            _options = options.Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (!_options.IsEnabled)
            {
                return HealthCheckResult.Healthy("Multiplayer presence is disabled.");
            }

            var tenantId = (int?)null;
            var matchId = Guid.NewGuid();
            var connectionId = $"health-{Guid.NewGuid():N}";
            var entry = new MultiplayerPresenceEntry
            {
                MatchId = matchId,
                ConnectionId = connectionId,
                InstanceId = _options.InstanceId,
                JoinedAt = DateTimeOffset.UtcNow,
                LastSeenAt = DateTimeOffset.UtcNow
            };

            try
            {
                var ttl = TimeSpan.FromSeconds(
                    Math.Max(1, Math.Min(5, _options.EntryTtlSeconds)));
                await _presenceStore.RegisterAsync(entry, ttl);
                var result = await _presenceStore.GetAsync(tenantId, matchId, connectionId);
                await _presenceStore.RemoveAsync(tenantId, matchId, connectionId);

                return result == null
                    ? HealthCheckResult.Unhealthy("Presence cache round-trip returned no value.")
                    : HealthCheckResult.Healthy();
            }
            catch (Exception exception)
            {
                return HealthCheckResult.Unhealthy("Presence cache is unavailable.", exception);
            }
        }
    }
}
