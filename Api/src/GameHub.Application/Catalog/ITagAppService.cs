using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Admin.Dto;
using GameHub.Catalog.Dto;

namespace GameHub.Catalog
{
    public interface ITagAppService : IApplicationService
    {
        Task<ListResultDto<TagDto>> GetAllAsync();

        Task<TagDto> GetAsync(Guid id);

        Task<TagDto> CreateOrUpdateAsync(CreateOrUpdateTagInput input);

        Task DeleteAsync(Guid id);
    }
}
