using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Eaf.Middleware.Web.Authentication;
using GameHub.Security;

namespace GameHub.Web.Security
{
    /// <summary>
    /// Generates short-lived game-scoped JWTs using the EAF token authentication service.
    /// </summary>
    public class GameTokenProvider : IGameTokenProvider
    {
        private readonly ITokenAuthenticationService _tokenAuthenticationService;

        public GameTokenProvider(ITokenAuthenticationService tokenAuthenticationService)
        {
            _tokenAuthenticationService = tokenAuthenticationService;
        }

        public async Task<string> CreateTokenAsync(long userId, int? tenantId, Guid gameId, TimeSpan expiration)
        {
            var claims = new List<Claim>
            {
                new Claim("sub", userId.ToString()),
                new Claim("gameId", gameId.ToString()),
                new Claim("tenantId", tenantId?.ToString() ?? "0")
            };

            return await _tokenAuthenticationService.CreateAccessTokenAsync(claims, expiration);
        }

        public async Task<string> CreatePreviewTokenAsync(long userId, int? tenantId, Guid gameId, string version, TimeSpan expiration)
        {
            var claims = new List<Claim>
            {
                new Claim("sub", userId.ToString()),
                new Claim("gameId", gameId.ToString()),
                new Claim("tenantId", tenantId?.ToString() ?? "0"),
                new Claim("version", version),
                new Claim("preview", "true")
            };

            return await _tokenAuthenticationService.CreateAccessTokenAsync(claims, expiration);
        }
    }
}
