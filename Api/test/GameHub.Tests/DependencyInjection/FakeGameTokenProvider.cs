using System;
using System.Threading.Tasks;
using GameHub.Security;

namespace GameHub.Tests.DependencyInjection
{
    public class FakeGameTokenProvider : IGameTokenProvider
    {
        public Task<string> CreateTokenAsync(long userId, int? tenantId, Guid gameId, TimeSpan expiration)
        {
            return Task.FromResult($"fake-token-{userId}-{gameId}");
        }

        public Task<string> CreatePreviewTokenAsync(long userId, int? tenantId, Guid gameId, string version, TimeSpan expiration)
        {
            return Task.FromResult($"fake-preview-token-{userId}-{gameId}-{version}");
        }

        public Task<GameTokenClaims> ValidateTokenAsync(string token, Guid? expectedGameId = null)
        {
            if (string.IsNullOrWhiteSpace(token) || !token.StartsWith("fake-token-", StringComparison.Ordinal))
            {
                return Task.FromResult<GameTokenClaims>(null);
            }

            var prefix = "fake-token-";
            var value = token.Substring(prefix.Length);
            var separator = value.IndexOf('-');
            if (separator <= 0
                || !long.TryParse(value.Substring(0, separator), out var userId)
                || !Guid.TryParse(value.Substring(separator + 1), out var gameId))
            {
                return Task.FromResult<GameTokenClaims>(null);
            }

            if (expectedGameId.HasValue && expectedGameId.Value != gameId)
            {
                return Task.FromResult<GameTokenClaims>(null);
            }

            return Task.FromResult<GameTokenClaims>(new GameTokenClaims
            {
                UserId = userId,
                GameId = gameId
            });
        }
    }
}
