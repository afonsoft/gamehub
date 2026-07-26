using System;

namespace GameHub.Multiplayer
{
    /// <summary>
    /// Ephemeral metadata for a multiplayer connection.
    /// </summary>
    [Serializable]
    public class MultiplayerPresenceEntry
    {
        public int? TenantId { get; set; }

        public Guid GameId { get; set; }

        public Guid MatchId { get; set; }

        public string ConnectionId { get; set; }

        public long UserId { get; set; }

        public string InstanceId { get; set; }

        public DateTimeOffset JoinedAt { get; set; }

        public DateTimeOffset LastSeenAt { get; set; }
    }
}
