using GameHub.Administration.Dashboard.Dto;
using System.Threading.Tasks;

namespace GameHub.Administration.Dashboard
{
    /// <summary>
    /// Serviço de aplicação para obtenção de dados do dashboard.
    /// </summary>
    public interface IDashboardAppService
    {
        /// <summary>
        /// Obtém os dados do dashboard para o host.
        /// </summary>
        /// <returns>Dados do dashboard do host.</returns>
        Task<DashboardOutput> GetHostDashboardAsync();

        /// <summary>
        /// Obtém os dados do dashboard para o tenant atual.
        /// </summary>
        /// <returns>Dados do dashboard do tenant.</returns>
        Task<DashboardOutput> GetTenantDashboardAsync();
    }
}
