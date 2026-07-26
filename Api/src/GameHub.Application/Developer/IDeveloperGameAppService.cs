using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Builds.Dto;
using GameHub.Catalog.Dto;
using GameHub.Developer.Dto;
using GameHub.Inspector.Dto;

namespace GameHub.Developer
{
    public interface IDeveloperGameAppService : IApplicationService
    {
        Task<GameDetailDto> CreateDraftAsync(CreateGameDraftInput input);

        Task<GameDetailDto> UpdateMetadataAsync(UpdateGameMetadataInput input);

        Task<GameDetailDto> SubmitForReviewAsync(SubmitGameForReviewInput input);

        Task<PagedResultDto<GameSummaryDto>> GetMyGamesAsync(GetGamesInput input);

        Task<ListResultDto<BuildDto>> GetBuildsAsync(Guid gameId);

        Task<ListResultDto<BuildDto>> GetVersionsAsync(Guid gameId);

        Task<List<DeveloperReviewHistoryItemDto>> GetReviewHistoryAsync(Guid gameId);

        Task<BuildDto> ApproveBuildAsync(DeveloperApproveBuildInput input);

        Task<BuildDto> RejectBuildAsync(DeveloperRejectBuildInput input);

        Task<UploadImageResultDto> UploadThumbnailAsync(Guid gameId, byte[] fileBytes, string fileName, string contentType);

        Task<UploadImageResultDto> UploadAnimatedThumbnailAsync(Guid gameId, byte[] fileBytes, string fileName, string contentType);

        Task<UploadImageResultDto> UploadHeroAsync(Guid gameId, byte[] fileBytes, string fileName, string contentType);

        Task<CreatePreviewTokenResult> CreatePreviewTokenForBuildAsync(CreatePreviewTokenInput input);

        Task<InspectorSessionDto> StartInspectorSessionForBuildAsync(StartInspectorSessionInput input);
    }
}
