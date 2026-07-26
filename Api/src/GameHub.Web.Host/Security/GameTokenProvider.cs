using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Eaf.Middleware.Web.Authentication;
using GameHub.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GameHub.Web.Security
{
    /// <summary>
    /// Generates short-lived game-scoped JWTs using the EAF token authentication service.
    /// </summary>
    public class GameTokenProvider : IGameTokenProvider
    {
        private readonly ITokenAuthenticationService _tokenAuthenticationService;
        private readonly IConfiguration _configuration;

        public GameTokenProvider(
            ITokenAuthenticationService tokenAuthenticationService,
            IConfiguration configuration)
        {
            _tokenAuthenticationService = tokenAuthenticationService;
            _configuration = configuration;
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

        public Task<GameTokenClaims> ValidateTokenAsync(string token, Guid? expectedGameId = null)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return Task.FromResult<GameTokenClaims>(null);
            }

            try
            {
                var key = _configuration["Authentication:JwtBearer:SecurityKey"];
                var issuer = _configuration["Authentication:JwtBearer:Issuer"];
                if (string.IsNullOrWhiteSpace(key) || key.StartsWith("REPLACE_", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult<GameTokenClaims>(null);
                }

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                    ValidIssuer = issuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(5)
                };

                var principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
                var userIdValue = principal.FindFirst("sub")?.Value;
                var gameIdValue = principal.FindFirst("gameId")?.Value;
                if (!long.TryParse(userIdValue, out var userId)
                    || !Guid.TryParse(gameIdValue, out var gameId)
                    || string.IsNullOrWhiteSpace(principal.FindFirst("tenantId")?.Value)
                    || (expectedGameId.HasValue && expectedGameId.Value != gameId))
                {
                    return Task.FromResult<GameTokenClaims>(null);
                }

                int? tenantId = null;
                if (int.TryParse(principal.FindFirst("tenantId")?.Value, out var parsedTenantId) && parsedTenantId != 0)
                {
                    tenantId = parsedTenantId;
                }

                return Task.FromResult<GameTokenClaims>(new GameTokenClaims
                {
                    UserId = userId,
                    TenantId = tenantId,
                    GameId = gameId
                });
            }
            catch (SecurityTokenException)
            {
                return Task.FromResult<GameTokenClaims>(null);
            }
        }
    }
}
