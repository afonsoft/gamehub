using System.Diagnostics.Metrics;

namespace GameHub.Monitoring
{
    /// <summary>
    /// OpenTelemetry instruments for multiplayer and arbitrary user data.
    /// </summary>
    public static class GameHubMetrics
    {
        public static readonly Meter Meter = new("GameHub");
        public static readonly Counter<long> MatchesCreated = Meter.CreateCounter<long>("multiplayer.matches.created");
        public static readonly ObservableGauge<int> ActiveMatches = Meter.CreateObservableGauge(
            "multiplayer.matches.active", () => 0);
        public static readonly Counter<long> PlayersConnected = Meter.CreateCounter<long>("multiplayer.players.connected");
        public static readonly Counter<long> MessagesSent = Meter.CreateCounter<long>("multiplayer.messages.sent");
        public static readonly Histogram<double> QueueWaitSeconds = Meter.CreateHistogram<double>("multiplayer.queue.wait_seconds");
        public static readonly Counter<long> MatchesCompleted = Meter.CreateCounter<long>("multiplayer.matches.completed");
        public static readonly Counter<long> MatchesAbandoned = Meter.CreateCounter<long>("multiplayer.matches.abandoned");
        public static readonly Histogram<long> LatencyMs = Meter.CreateHistogram<long>("multiplayer.latency_ms");
        public static readonly Counter<long> AudsKeysStored = Meter.CreateCounter<long>("auds.keys.stored");
        public static readonly Counter<long> AudsBytesStored = Meter.CreateCounter<long>("auds.bytes.stored");
        public static readonly Counter<long> PresenceRegistered = Meter.CreateCounter<long>("multiplayer.presence.registered");
        public static readonly Counter<long> PresenceRefreshed = Meter.CreateCounter<long>("multiplayer.presence.refreshed");
        public static readonly Counter<long> PresenceRemoved = Meter.CreateCounter<long>("multiplayer.presence.removed");
        public static readonly Counter<long> PresenceCacheErrors = Meter.CreateCounter<long>("multiplayer.presence.cache_errors");
        public static readonly Histogram<double> PresenceOperationDurationMs =
            Meter.CreateHistogram<double>("multiplayer.presence.operation_duration_ms");
    }
}
