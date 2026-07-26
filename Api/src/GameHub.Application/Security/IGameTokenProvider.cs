using System;
using System.Threading.Tasks;

namespace GameHub.Security
{
    /// <summary>
    /// Claims carried by a short-lived game token.
    /// </summary>
    public sealed class GameTokenClaims
    {
        public long UserId { get; set; }

        public int? TenantId { get; set; }

        public Guid GameId { get; set; }
    }

    /// <summary>
    /// Generates short-lived game-scoped access tokens for the current player.
    /// </summary>
    public interface IGameTokenProvider
    {
        /// <summary>Creates a JWT containing sub, gameId, tenantId and exp claims.</summary>
        Task<string> CreateTokenAsync(long userId, int? tenantId, Guid gameId, TimeSpan expiration);

        /// <summary>Creates a JWT containing sub, gameId, version, preview and exp claims.</summary>
        Task<string> CreatePreviewTokenAsync(long userId, int? tenantId, Guid gameId, string version, TimeSpan expiration);

        /// <summary>
        /// Validates a game token and optionally enforces the game scope.
        /// </summary>
        Task<GameTokenClaims> ValidateTokenAsync(string token, Guid? expectedGameId = null);
    }
}
