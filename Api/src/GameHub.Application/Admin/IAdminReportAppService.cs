using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Admin.Dto;

namespace GameHub.Admin
{
    /// <summary>
    /// Contrato de serviço de gestão de reports.
    /// </summary>
    public interface IAdminReportAppService : IApplicationService
    {
        Task<PagedResultDto<UserReportDto>> GetAllAsync(GetReportsInput input);

        Task UpdateStatusAsync(Guid reportId, string status);
    }
}
