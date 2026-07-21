using System;
using System.Threading;
using System.Threading.Tasks;
using GameHub.Catalog.Dto;

namespace GameHub.Catalog
{
    public class InMemoryGameCatalogCache : IGameCatalogCache
    {
        private HomeResponseDto _cached;
        private DateTime? _expiresAt;

        public Task<HomeResponseDto> GetHomeAsync(CancellationToken cancellationToken = default)
        {
            if (_expiresAt.HasValue && _expiresAt.Value > DateTime.UtcNow)
            {
                return Task.FromResult(_cached);
            }

            return Task.FromResult<HomeResponseDto>(null);
        }

        public Task SetHomeAsync(HomeResponseDto dto, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            _cached = dto;
            _expiresAt = DateTime.UtcNow.Add(ttl);
            return Task.CompletedTask;
        }

        public Task InvalidateHomeAsync(CancellationToken cancellationToken = default)
        {
            _cached = null;
            _expiresAt = null;
            return Task.CompletedTask;
        }
    }
}
