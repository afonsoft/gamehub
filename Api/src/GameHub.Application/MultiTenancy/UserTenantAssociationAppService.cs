using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using GameHub.Authorization;
using GameHub.MultiTenancy.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.MultiTenancy
{
    [AbpAuthorize(GameHubPermissions.Pages_Users)]
    public class UserTenantAssociationAppService : GameHubAppServiceBase, IUserTenantAssociationAppService
    {
        private readonly ITenantUserManager _tenantUserManager;
        private readonly IRepository<UserTenantMembership, long> _membershipRepository;
        private readonly IRepository<Eaf.Middleware.MultiTenancy.Tenant, int> _tenantRepository;

        public UserTenantAssociationAppService(
            ITenantUserManager tenantUserManager,
            IRepository<UserTenantMembership, long> membershipRepository,
            IRepository<Eaf.Middleware.MultiTenancy.Tenant, int> tenantRepository)
        {
            _tenantUserManager = tenantUserManager;
            _membershipRepository = membershipRepository;
            _tenantRepository = tenantRepository;
        }

        [AbpAuthorize(GameHubPermissions.Pages_Users_Manage)]
        public virtual async Task<UserTenantMembershipDto> AssociateAsync(AssociateUserToTenantInput input)
        {
            var membership = await _tenantUserManager.EnsureMembershipAsync(input.UserId, input.TenantId, input.IsDefault);
            return await MapToDtoAsync(membership);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Users_Manage)]
        public virtual async Task RemoveAssociationAsync(RemoveUserTenantAssociationInput input)
        {
            await _tenantUserManager.RemoveMembershipAsync(input.UserId, input.TenantId);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Users_Manage)]
        public virtual async Task<UserTenantMembershipDto> SetDefaultAsync(SetDefaultTenantInput input)
        {
            var membership = await _tenantUserManager.EnsureMembershipAsync(input.UserId, input.TenantId, true);
            return await MapToDtoAsync(membership);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Users)]
        public virtual async Task<List<UserTenantMembershipDto>> GetUserMembershipsAsync(GetUserTenantMembershipsInput input)
        {
            var memberships = await _membershipRepository.GetAll()
                .Where(m => m.UserId == input.UserId)
                .ToListAsync();

            var tenantIds = memberships.Select(m => m.TenantId).Distinct().ToList();
            var tenants = await _tenantRepository.GetAll()
                .Where(t => tenantIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id);

            var result = new List<UserTenantMembershipDto>(memberships.Count);
            foreach (var m in memberships)
            {
                var dto = ObjectMapper.Map<UserTenantMembershipDto>(m);
                if (tenants.TryGetValue(m.TenantId, out var tenant))
                {
                    dto.TenantName = tenant.Name;
                    dto.TenantTenancyName = tenant.TenancyName;
                }

                result.Add(dto);
            }

            return result;
        }

        private async Task<UserTenantMembershipDto> MapToDtoAsync(UserTenantMembership membership)
        {
            var dto = ObjectMapper.Map<UserTenantMembershipDto>(membership);
            var tenant = await _tenantRepository.FirstOrDefaultAsync(t => t.Id == membership.TenantId);
            if (tenant != null)
            {
                dto.TenantName = tenant.Name;
                dto.TenantTenancyName = tenant.TenancyName;
            }

            return dto;
        }
    }
}
