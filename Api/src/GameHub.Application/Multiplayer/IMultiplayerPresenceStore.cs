using System;
using System.Threading.Tasks;

namespace GameHub.Multiplayer
{
    /// <summary>
    /// Stores ephemeral multiplayer connection presence.
    /// </summary>
    public interface IMultiplayerPresenceStore
    {
        /// <summary>
        /// Registers or replaces a connection presence entry.
        /// </summary>
        Task RegisterAsync(MultiplayerPresenceEntry entry, TimeSpan ttl);

        /// <summary>
        /// Gets a connection presence entry.
        /// </summary>
        Task<MultiplayerPresenceEntry> GetAsync(int? tenantId, Guid matchId, string connectionId);

        /// <summary>
        /// Gets a connection presence entry without requiring the match id.
        /// </summary>
        Task<MultiplayerPresenceEntry> GetByConnectionAsync(int? tenantId, string connectionId);

        Task<MultiplayerPresenceEntry> GetByUserAsync(int? tenantId, long userId);

        /// <summary>
        /// Refreshes the TTL and last activity timestamp of a connection.
        /// </summary>
        Task RefreshAsync(int? tenantId, Guid matchId, string connectionId, TimeSpan ttl);

        /// <summary>
        /// Removes a connection presence entry.
        /// </summary>
        Task RemoveAsync(int? tenantId, Guid matchId, string connectionId);

        /// <summary>
        /// Removes a connection presence entry without requiring the match id.
        /// </summary>
        Task RemoveByConnectionAsync(int? tenantId, string connectionId);
    }
}
