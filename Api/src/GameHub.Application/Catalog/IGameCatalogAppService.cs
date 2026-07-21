using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Catalog.Dto;

namespace GameHub.Catalog
{
    public interface IGameCatalogAppService : IApplicationService
    {
        Task<HomeResponseDto> GetHomeAsync();

        Task<PagedResultDto<GameCardDto>> GetGamesAsync(GetGamesInput input);

        Task<GameDetailDto> GetBySlugAsync(string slug);

        Task<SearchResultDto> SearchAsync(SearchInput input);

        Task<ListResultDto<GameCardDto>> GetRelatedAsync(Guid gameId);
    }
}
