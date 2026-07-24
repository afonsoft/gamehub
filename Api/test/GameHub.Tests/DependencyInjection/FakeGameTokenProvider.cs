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
    }
}
