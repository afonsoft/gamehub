using System;
using System.Threading;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using GameHub.Catalog.Dto;

namespace GameHub.Catalog
{
    public interface IGameCatalogCache
    {
        Task<HomeResponseDto> GetHomeAsync(CancellationToken cancellationToken = default);

        Task SetHomeAsync(HomeResponseDto dto, TimeSpan ttl, CancellationToken cancellationToken = default);

        Task InvalidateHomeAsync(CancellationToken cancellationToken = default);

        Task<GameDetailDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

        Task SetBySlugAsync(string slug, GameDetailDto dto, TimeSpan ttl, CancellationToken cancellationToken = default);

        Task InvalidateBySlugAsync(string slug, CancellationToken cancellationToken = default);

        Task<SearchResultDto> GetSearchAsync(string cacheKey, CancellationToken cancellationToken = default);

        Task SetSearchAsync(string cacheKey, SearchResultDto dto, TimeSpan ttl, CancellationToken cancellationToken = default);

        Task InvalidateSearchAsync(CancellationToken cancellationToken = default);

        Task<ListResultDto<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);

        Task SetCategoriesAsync(ListResultDto<CategoryDto> dto, TimeSpan ttl, CancellationToken cancellationToken = default);

        Task InvalidateCategoriesAsync(CancellationToken cancellationToken = default);

        Task<ListResultDto<TagDto>> GetTagsAsync(CancellationToken cancellationToken = default);

        Task SetTagsAsync(ListResultDto<TagDto> dto, TimeSpan ttl, CancellationToken cancellationToken = default);

        Task InvalidateTagsAsync(CancellationToken cancellationToken = default);
    }
}
