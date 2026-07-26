using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Admin.Dto;
using GameHub.Catalog.Dto;

namespace GameHub.Admin
{
    public interface IAdminGameAppService : IApplicationService
    {
        Task<PagedResultDto<AdminGameListItemDto>> GetAllAsync(GetGamesInput input);

        Task<AdminGameDetailDto> GetDetailAsync(Guid gameId);

        Task PublishAsync(PublishGameInput input);

        Task SuspendAsync(SuspendGameInput input);

        Task ApproveThumbnailAsync(Guid gameId);

        Task RejectThumbnailAsync(Guid gameId);

        Task<List<CategoryDto>> SuggestCategoriesAsync(SuggestCategoriesInput input);

        Task<ValidateSeoResultDto> ValidateSeoAsync(ValidateSeoInput input);
    }
}
