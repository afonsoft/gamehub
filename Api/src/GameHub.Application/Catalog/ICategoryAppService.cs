using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Admin.Dto;
using GameHub.Catalog.Dto;

namespace GameHub.Catalog
{
    public interface ICategoryAppService : IApplicationService
    {
        Task<ListResultDto<CategoryDto>> GetAllAsync();

        Task<CategoryDto> GetAsync(Guid id);

        Task<CategoryDto> CreateOrUpdateAsync(CreateOrUpdateCategoryInput input);

        Task DeleteAsync(Guid id);
    }
}
