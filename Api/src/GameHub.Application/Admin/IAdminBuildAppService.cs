using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Admin.Dto;

namespace GameHub.Admin
{
    /// <summary>
    /// Admin service for inspecting uploaded game builds and their extracted files.
    /// </summary>
    public interface IAdminBuildAppService : IApplicationService
    {
        /// <summary>
        /// Lists all uploaded builds across games with optional filters.
        /// </summary>
        Task<PagedResultDto<AdminBuildListItemDto>> GetAllBuildsAsync(GetBuildsInput input);

        /// <summary>
        /// Lists the extracted files of a specific build.
        /// </summary>
        Task<ListResultDto<BuildFileDto>> GetBuildFilesAsync(Guid buildId);
    }
}
