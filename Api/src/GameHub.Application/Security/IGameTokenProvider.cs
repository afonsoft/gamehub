using System;
using System.Threading.Tasks;

namespace GameHub.Security
{
    /// <summary>
    /// Generates short-lived game-scoped access tokens for the current player.
    /// </summary>
    public interface IGameTokenProvider
    {
        /// <summary>Creates a JWT containing sub, gameId, tenantId and exp claims.</summary>
        Task<string> CreateTokenAsync(long userId, int? tenantId, Guid gameId, TimeSpan expiration);

        /// <summary>Creates a JWT containing sub, gameId, version, preview and exp claims.</summary>
        Task<string> CreatePreviewTokenAsync(long userId, int? tenantId, Guid gameId, string version, TimeSpan expiration);
    }
}
