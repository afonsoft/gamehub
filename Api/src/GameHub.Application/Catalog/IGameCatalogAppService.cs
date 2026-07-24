using System;
using System.Threading;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Catalog.Dto;

namespace GameHub.Catalog
{
    public interface IGameCatalogAppService : IApplicationService
    {
        Task<HomeResponseDto> GetHomeAsync(CancellationToken cancellationToken = default);

        Task<PagedResultDto<GameCardDto>> GetGamesAsync(GetGamesInput input, CancellationToken cancellationToken = default);

        Task<CategoryDto> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default);

        Task<GameDetailDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

        Task<GameVoteResultDto> GetVoteAsync(Guid gameId, string deviceId = null);

        Task<GameVoteResultDto> VoteAsync(GameVoteInput input);

        Task<SearchResultDto> SearchAsync(SearchInput input, CancellationToken cancellationToken = default);

        Task<ListResultDto<GameCardDto>> GetRelatedAsync(Guid gameId, CancellationToken cancellationToken = default);
    }
}
