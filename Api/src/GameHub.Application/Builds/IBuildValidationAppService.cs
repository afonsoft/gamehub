using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Builds.Dto;

namespace GameHub.Builds
{
    /// <summary>
    /// Service for retrieving build validation reports.
    /// </summary>
    public interface IBuildValidationAppService : IApplicationService
    {
        Task<BuildValidationReportDto> GetReportAsync(Guid gameBuildId);

        Task<List<BuildValidationReportListItemDto>> GetReportsAsync(int? maxResultCount = 50);
    }
}
