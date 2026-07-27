using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.MultiTenancy.Dto;

namespace GameHub.MultiTenancy
{
    public interface IUserTenantAssociationAppService : IApplicationService
    {
        Task<UserTenantMembershipDto> AssociateAsync(AssociateUserToTenantInput input);
        Task RemoveAssociationAsync(RemoveUserTenantAssociationInput input);
        Task<UserTenantMembershipDto> SetDefaultAsync(SetDefaultTenantInput input);
        Task<List<UserTenantMembershipDto>> GetUserMembershipsAsync(GetUserTenantMembershipsInput input);
    }
}
