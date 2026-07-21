using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Catalog.Dto;
using GameHub.Developer.Dto;

namespace GameHub.Developer
{
    public interface IDeveloperGameAppService : IApplicationService
    {
        Task<GameDetailDto> CreateDraftAsync(CreateGameDraftInput input);

        Task<GameDetailDto> UpdateMetadataAsync(UpdateGameMetadataInput input);

        Task<GameDetailDto> SubmitForReviewAsync(SubmitGameForReviewInput input);

        Task<PagedResultDto<GameSummaryDto>> GetMyGamesAsync(GetGamesInput input);

        Task<ListResultDto<BuildDto>> GetBuildsAsync(Guid gameId);
    }
}
