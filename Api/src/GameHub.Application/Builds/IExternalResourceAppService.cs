using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Builds.Dto;

namespace GameHub.Builds
{
    public interface IExternalResourceAppService : IApplicationService
    {
        Task<ExternalResourceExemptionDto> RequestExemptionAsync(RequestExternalResourceExemptionInput input);

        Task<List<ExternalResourceExemptionDto>> GetByGameAsync(Guid gameId);

        Task<ExternalResourceExemptionDto> ReviewAsync(ReviewExternalResourceExemptionInput input);
    }
}
