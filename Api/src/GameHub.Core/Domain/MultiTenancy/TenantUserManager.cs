using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Abp.UI;
using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using UserManager = Eaf.Middleware.Authorization.Users.UserManager;

namespace GameHub.MultiTenancy
{
    public class TenantUserManager : GameHubDomainServiceBase, ITenantUserManager
    {
        private readonly IRepository<UserTenantMembership, long> _membershipRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Tenant, int> _tenantRepository;
        private readonly UserManager _userManager;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public TenantUserManager(
            IRepository<UserTenantMembership, long> membershipRepository,
            IRepository<User, long> userRepository,
            IRepository<Tenant, int> tenantRepository,
            UserManager userManager,
            IPasswordHasher<User> passwordHasher,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _membershipRepository = membershipRepository;
            _userRepository = userRepository;
            _tenantRepository = tenantRepository;
            _userManager = userManager;
            _passwordHasher = passwordHasher;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public virtual async Task<UserTenantMembership> EnsureMembershipAsync(long hostUserId, int tenantId, bool isDefault = false)
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var hostUser = await _userRepository.GetAsync(hostUserId);
                if (hostUser.TenantId != null)
                    throw new UserFriendlyException(L("OnlyHostUsersCanBeAssociatedWithMultipleTenants"));

                await _tenantRepository.GetAsync(tenantId);

                var existing = await _membershipRepository.FirstOrDefaultAsync(m => m.UserId == hostUserId && m.TenantId == tenantId);
                if (existing != null)
                {
                    if (isDefault)
                    {
                        await ClearDefaultFlagAsync(hostUserId);
                        existing.IsDefault = true;
                        await _membershipRepository.UpdateAsync(existing);
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }

                    await UpdateShadowUserAsync(existing.TenantUserId, tenantId, hostUser);
                    return existing;
                }

                if (isDefault)
                    await ClearDefaultFlagAsync(hostUserId);

                var shadowUser = await CreateOrUpdateShadowUserAsync(hostUser, tenantId, null);

                var membership = new UserTenantMembership
                {
                    UserId = hostUserId,
                    TenantId = tenantId,
                    TenantUserId = shadowUser.Id,
                    IsDefault = isDefault,
                };

                await _membershipRepository.InsertAsync(membership);
                await CurrentUnitOfWork.SaveChangesAsync();
                return membership;
            }
        }

        public virtual async Task RemoveMembershipAsync(long hostUserId, int tenantId)
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var membership = await _membershipRepository.FirstOrDefaultAsync(m => m.UserId == hostUserId && m.TenantId == tenantId);
                if (membership == null)
                    return;

                using (CurrentUnitOfWork.SetTenantId(tenantId))
                {
                    var shadowUser = await _userRepository.GetAsync(membership.TenantUserId);
                    await _userManager.DeleteAsync(shadowUser);
                }

                await _membershipRepository.DeleteAsync(membership);
            }
        }

        private async Task<User> CreateOrUpdateShadowUserAsync(User hostUser, int tenantId, long? existingShadowUserId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                User shadowUser;
                if (existingShadowUserId.HasValue)
                {
                    shadowUser = await _userRepository.GetAsync(existingShadowUserId.Value);
                }
                else
                {
                    shadowUser = await _userRepository.FirstOrDefaultAsync(u =>
                        u.UserName == hostUser.UserName && u.TenantId == tenantId);
                }

                var hostRoles = await _userManager.GetRolesAsync(hostUser);

                if (shadowUser == null)
                {
                    shadowUser = new User
                    {
                        TenantId = tenantId,
                        UserName = hostUser.UserName,
                        Name = hostUser.Name,
                        Surname = hostUser.Surname,
                        EmailAddress = hostUser.EmailAddress,
                        IsEmailConfirmed = hostUser.IsEmailConfirmed,
                        IsActive = hostUser.IsActive,
                        Password = hostUser.Password,
                        SecurityStamp = hostUser.SecurityStamp,
                    };

                    shadowUser.SetNormalizedNames();
                    await _userRepository.InsertAsync(shadowUser);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else
                {
                    shadowUser.Name = hostUser.Name;
                    shadowUser.Surname = hostUser.Surname;
                    shadowUser.EmailAddress = hostUser.EmailAddress;
                    shadowUser.IsActive = hostUser.IsActive;
                    shadowUser.Password = hostUser.Password;
                    shadowUser.SecurityStamp = hostUser.SecurityStamp;
                    shadowUser.SetNormalizedNames();

                    await _userRepository.UpdateAsync(shadowUser);
                    await CurrentUnitOfWork.SaveChangesAsync();

                    var currentRoles = await _userManager.GetRolesAsync(shadowUser);
                    foreach (var role in currentRoles.Except(hostRoles))
                    {
                        (await _userManager.RemoveFromRoleAsync(shadowUser, role)).CheckErrors();
                    }
                }

                foreach (var role in hostRoles)
                {
                    if (!await _userManager.IsInRoleAsync(shadowUser, role))
                    {
                        (await _userManager.AddToRoleAsync(shadowUser, role)).CheckErrors();
                    }
                }

                return shadowUser;
            }
        }

        private async Task UpdateShadowUserAsync(long shadowUserId, int tenantId, User hostUser)
        {
            await CreateOrUpdateShadowUserAsync(hostUser, tenantId, shadowUserId);
        }

        private async Task ClearDefaultFlagAsync(long hostUserId)
        {
            var defaults = await _membershipRepository.GetAllListAsync(m => m.UserId == hostUserId && m.IsDefault);
            foreach (var d in defaults)
            {
                d.IsDefault = false;
                await _membershipRepository.UpdateAsync(d);
            }
        }
    }
}
