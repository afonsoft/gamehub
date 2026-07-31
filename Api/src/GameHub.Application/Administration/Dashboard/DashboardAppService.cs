using Abp.Application.Editions;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Eaf.Middleware.Authorization.Users;
using GameHub.Administration.Dashboard.Dto;
using Eaf.Middleware.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameHub.Administration.Dashboard
{
    /// <summary>
    /// Serviço de aplicação que retorna estatísticas para o dashboard.
    /// </summary>
    [AbpAuthorize("Pages.Dashboard")]
    public class DashboardAppService : GameHubAppServiceBase, IDashboardAppService
    {
        private readonly IRepository<Tenant, int> _tenantRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Edition, int> _editionRepository;

        /// <summary>
        /// DashboardAppService.
        /// </summary>
        /// <param name="tenantRepository">Repositório de tenants.</param>
        /// <param name="userRepository">Repositório de usuários.</param>
        /// <param name="editionRepository">Repositório de edições.</param>
        public DashboardAppService(
            IRepository<Tenant, int> tenantRepository,
            IRepository<User, long> userRepository,
            IRepository<Edition, int> editionRepository)
        {
            _tenantRepository = tenantRepository;
            _userRepository = userRepository;
            _editionRepository = editionRepository;
        }

        /// <summary>
        /// Obtém os dados do dashboard para o host.
        /// </summary>
        /// <returns>Dados do dashboard do host.</returns>
        public virtual async Task<DashboardOutput> GetHostDashboardAsync()
        {
            var output = new DashboardOutput { IsHostDashboard = true };

            output.Tiles.Add(new DashboardTileDto
            {
                Id = "totalTenants",
                Title = L("TotalTenants"),
                Count = await _tenantRepository.CountAsync(),
                Style = "primary",
                Icon = "flaticon-users-1"
            });

            output.Tiles.Add(new DashboardTileDto
            {
                Id = "totalUsers",
                Title = L("TotalUsers"),
                Count = await _userRepository.CountAsync(),
                Style = "success",
                Icon = "flaticon-users"
            });

            output.Tiles.Add(new DashboardTileDto
            {
                Id = "totalEditions",
                Title = L("TotalEditions"),
                Count = await _editionRepository.CountAsync(),
                Style = "warning",
                Icon = "flaticon-layers"
            });

            return output;
        }

        /// <summary>
        /// Obtém os dados do dashboard para o tenant atual.
        /// </summary>
        /// <returns>Dados do dashboard do tenant.</returns>
        public virtual async Task<DashboardOutput> GetTenantDashboardAsync()
        {
            var output = new DashboardOutput { IsHostDashboard = false };

            using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
            {
                output.Tiles.Add(new DashboardTileDto
                {
                    Id = "totalUsers",
                    Title = L("TotalUsers"),
                    Count = await _userRepository.CountAsync(),
                    Style = "success",
                    Icon = "flaticon-users"
                });
            }

            return output;
        }
    }
}
