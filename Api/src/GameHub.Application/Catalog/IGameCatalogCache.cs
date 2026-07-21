using System;
using System.Threading;
using System.Threading.Tasks;
using GameHub.Catalog.Dto;

namespace GameHub.Catalog
{
    public interface IGameCatalogCache
    {
        Task<HomeResponseDto> GetHomeAsync(CancellationToken cancellationToken = default);

        Task SetHomeAsync(HomeResponseDto dto, TimeSpan ttl, CancellationToken cancellationToken = default);

        Task InvalidateHomeAsync(CancellationToken cancellationToken = default);
    }
}
