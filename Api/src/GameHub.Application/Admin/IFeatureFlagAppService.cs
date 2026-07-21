using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Admin.Dto;

namespace GameHub.Admin
{
    /// <summary>
    /// Contrato de serviço de feature flags.
    /// </summary>
    public interface IFeatureFlagAppService : IApplicationService
    {
        Task<ListResultDto<FeatureFlagDto>> GetAllAsync();

        Task<FeatureFlagDto> ToggleAsync(Guid id, bool isEnabled);
    }
}
