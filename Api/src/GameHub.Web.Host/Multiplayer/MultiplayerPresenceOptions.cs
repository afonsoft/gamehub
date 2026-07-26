using System;

namespace GameHub.Web.Multiplayer
{
    /// <summary>
    /// Configuration for multiplayer presence cache entries.
    /// </summary>
    public class MultiplayerPresenceOptions
    {
        public bool IsEnabled { get; set; } = true;

        public int EntryTtlSeconds { get; set; } = 90;

        public int HeartbeatIntervalSeconds { get; set; } = 30;

        public string InstanceId { get; set; } = Environment.MachineName;

        public TimeSpan EntryTtl => TimeSpan.FromSeconds(EntryTtlSeconds);
    }
}
