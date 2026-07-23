using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Developer.Dto;

namespace GameHub.Developer
{
    public interface IDeveloperDashboardAppService : IApplicationService
    {
        Task<DeveloperDashboardDto> GetDashboardAsync();
    }
}
