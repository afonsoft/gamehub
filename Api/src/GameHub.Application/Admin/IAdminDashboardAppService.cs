using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Admin.Dto;

namespace GameHub.Admin
{
    /// <summary>
    /// Contrato de serviço do dashboard administrativo.
    /// </summary>
    public interface IAdminDashboardAppService : IApplicationService
    {
        Task<AdminDashboardSummaryDto> GetSummaryAsync();

        Task<PlaysOverTimeResultDto> GetPlaysOverTimeAsync(int days);
    }
}
