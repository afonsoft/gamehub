using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Abp.Runtime.Security;
using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.Web.Authentication;
using Eaf.Middleware.Web.Authentication.JwtBearer;
using Eaf.Middleware.Web.Models.TokenAuth;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using GameHub.Web.Extensions;

namespace GameHub.Web.Authentication
{
    /// <summary>
    /// Default JWT token authentication service for the GameHub web host.
    /// Generates EAF-compatible access tokens with token validity claims.
    /// </summary>
    public class JwtTokenAuthenticationService : ITokenAuthenticationService
    {
        private readonly TokenAuthConfiguration _configuration;
        private readonly UserManager _userManager;
        private readonly ICacheManager _cacheManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<JwtTokenAuthenticationService> _logger;

        public JwtTokenAuthenticationService(
            TokenAuthConfiguration configuration,
            UserManager userManager,
            ICacheManager cacheManager,
            IUnitOfWorkManager unitOfWorkManager,
            ILogger<JwtTokenAuthenticationService> logger)
        {
            _configuration = configuration;
            _userManager = userManager;
            _cacheManager = cacheManager;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        public Task<string> CreateAccessTokenAsync(IEnumerable<Claim> claims, TimeSpan expiration)
        {
            var claimList = claims?.ToList() ?? new List<Claim>();
            if (IsGameScopedToken(claimList))
            {
                return WriteTokenAsync(claimList, expiration);
            }

            return CreateUserAccessTokenAsync(claimList, expiration, CancellationToken.None);
        }

        private async Task<string> CreateUserAccessTokenAsync(List<Claim> claimList, TimeSpan expiration, CancellationToken cancellationToken)
        {
            var userId = claimList.GetUserIdFromClaims();
            var tenantId = claimList.GetTenantIdFromClaims();

            if (!userId.HasValue)
            {
                _logger.LogWarning("Could not resolve user id from claims; generating unsigned game token envelope.");
                return await WriteTokenAsync(claimList, expiration);
            }

            User user;
            using (_unitOfWorkManager.Current.SetTenantId(tenantId))
            {
                user = await _userManager.FindByIdAsync(userId.Value.ToString())
                    ?? throw new SecurityTokenException($"User {userId.Value} not found in tenant {tenantId?.ToString() ?? "host"}.");

                if (string.IsNullOrWhiteSpace(user.SecurityStamp))
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                    user.SecurityStamp = await _userManager.GetSecurityStampAsync(user);
                }

                var tokenValidityKey = Guid.NewGuid().ToString();
                var userIdentifier = new UserIdentifier(tenantId, user.Id).ToUserIdentifierString();

                EnrichUserClaims(claimList, user, tokenValidityKey, userIdentifier);

                await _userManager.AddTokenValidityKeyAsync(
                    user,
                    tokenValidityKey,
                    DateTime.UtcNow.Add(expiration).AddSeconds(10),
                    cancellationToken);

                await _unitOfWorkManager.Current.SaveChangesAsync();

                var cache = _cacheManager.GetCache("token_validity_key");
                await cache.SetAsync(
                    tokenValidityKey,
                    (object)user.SecurityStamp,
                    slidingExpireTime: expiration,
                    absoluteExpireTime: DateTimeOffset.UtcNow.Add(expiration).AddHours(1));
            }

            return await WriteTokenAsync(claimList, expiration);
        }

        private static void EnrichUserClaims(List<Claim> claimList, User user, string tokenValidityKey, string userIdentifier)
        {
            claimList.RemoveAll(c => c.Type == "user_identifier" || c.Type == "token_validity_key" || c.Type == "token_validity_value");

            claimList.AddOrReplaceClaim(new Claim(AbpClaimTypes.UserId, user.Id.ToString()));
            claimList.AddOrReplaceClaim(new Claim("sub", user.Id.ToString()));
            claimList.AddOrReplaceClaim(new Claim(AbpClaimTypes.UserName, user.UserName));
            claimList.AddOrReplaceClaim(new Claim(System.Security.Claims.ClaimTypes.Email, user.EmailAddress ?? string.Empty));
            claimList.AddOrReplaceClaim(new Claim("AspNet.Identity.SecurityStamp", user.SecurityStamp));
            claimList.AddOrReplaceClaim(new Claim("token_validity_value", user.SecurityStamp));
            claimList.AddOrReplaceClaim(new Claim("user_identifier", userIdentifier));
            claimList.AddOrReplaceClaim(new Claim("token_validity_key", tokenValidityKey));
            claimList.AddOrReplaceClaim(new Claim("jti", Guid.NewGuid().ToString()));
            claimList.AddOrReplaceClaim(new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            claimList.AddOrReplaceClaim(new Claim("amr", "pwd"));
        }

        private static bool IsGameScopedToken(List<Claim> claimList)
        {
            return claimList.Any(c => c.Type == "gameId");
        }

        private Task<string> WriteTokenAsync(List<Claim> claimList, TimeSpan expiration)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claimList,
                notBefore: DateTime.UtcNow.AddMinutes(-1),
                expires: DateTime.UtcNow.Add(expiration),
                signingCredentials: _configuration.SigningCredentials
            );

            return Task.FromResult(handler.WriteToken(token));
        }

        public Task<AuthenticateResultModel> AuthenticateAsync(AuthenticateModel model)
        {
            throw new NotSupportedException("Use the hub authentication flow instead.");
        }
    }
}
