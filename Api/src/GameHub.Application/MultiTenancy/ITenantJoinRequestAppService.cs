using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.MultiTenancy.Dto;

namespace GameHub.MultiTenancy
{
    public interface ITenantJoinRequestAppService : IApplicationService
    {
        Task<List<AvailableTenantDto>> GetAvailableTenantsAsync();

        Task<TenantJoinRequestDto> CreateRequestAsync(CreateTenantJoinRequestInput input);

        Task<List<TenantJoinRequestDto>> GetMyRequestsAsync();

        Task<List<TenantJoinRequestDto>> GetPendingRequestsForCurrentTenantAsync();

        Task<TenantJoinRequestDto> ApproveAsync(ApproveTenantJoinRequestInput input);
    }
}
