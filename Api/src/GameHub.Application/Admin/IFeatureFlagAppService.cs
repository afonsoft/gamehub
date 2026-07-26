using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Gets enabled feature names for compatibility checks in the SDK.
        /// </summary>
        Task<List<string>> GetEnabledNamesAsync();

        Task<FeatureFlagDto> ToggleAsync(Guid id, bool isEnabled);
    }
}
