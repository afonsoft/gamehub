using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Data;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Abp.Timing;
using Abp.UI;
using Eaf.Middleware.Authorization.Roles;
using Eaf.Middleware.Authorization.Users;
using GameHub.Authorization;
using GameHub.Companies.Dto;
using GameHub.Developer.Dto;
using GameHub.Developers;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Companies
{
    [AbpAuthorize(GameHubPermissions.Pages_Company_Employees)]
    public class CompanyEmployeeAppService : GameHubAppServiceBase, ICompanyEmployeeAppService
    {
        private readonly Eaf.Middleware.MultiTenancy.ITenantUserManager _tenantUserManager;
        private readonly IRepository<Eaf.Middleware.MultiTenancy.UserTenantMembership, long> _membershipRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Eaf.Middleware.MultiTenancy.Tenant, int> _tenantRepository;
        private readonly IRepository<DeveloperTeam, Guid> _teamRepository;
        private readonly IRepository<DeveloperTeamMember, Guid> _teamMemberRepository;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;

        public CompanyEmployeeAppService(
            Eaf.Middleware.MultiTenancy.ITenantUserManager tenantUserManager,
            IRepository<Eaf.Middleware.MultiTenancy.UserTenantMembership, long> membershipRepository,
            IRepository<User, long> userRepository,
            IRepository<Eaf.Middleware.MultiTenancy.Tenant, int> tenantRepository,
            IRepository<DeveloperTeam, Guid> teamRepository,
            IRepository<DeveloperTeamMember, Guid> teamMemberRepository,
            UserManager userManager,
            RoleManager roleManager)
        {
            _tenantUserManager = tenantUserManager;
            _membershipRepository = membershipRepository;
            _userRepository = userRepository;
            _tenantRepository = tenantRepository;
            _teamRepository = teamRepository;
            _teamMemberRepository = teamMemberRepository;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<CompanyEmployeeDto>> GetEmployeesAsync(int tenantId)
        {
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var memberships = await _membershipRepository.GetAll()
                    .Where(m => m.TenantId == tenantId)
                    .ToListAsync();

                var team = await _teamRepository.FirstOrDefaultAsync(t => t.TenantId == tenantId);
                var teamMemberByUserId = new Dictionary<long, DeveloperTeamMember>();
                if (team != null)
                {
                    teamMemberByUserId = await _teamMemberRepository.GetAll()
                        .Where(m => m.TeamId == team.Id)
                        .ToDictionaryAsync(m => m.UserId);
                }

                var result = new List<CompanyEmployeeDto>();

                foreach (var membership in memberships)
                {
                    var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == membership.UserId);
                    if (user == null)
                    {
                        continue;
                    }

                    var role = DeveloperTeamRole.Developer.ToString();
                    if (teamMemberByUserId.TryGetValue(user.Id, out var teamMember))
                    {
                        role = teamMember.Role.ToString();
                    }

                    result.Add(new CompanyEmployeeDto
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        EmailAddress = user.EmailAddress,
                        Role = role,
                        IsDefault = membership.IsDefault,
                        JoinedAt = membership.CreationTime,
                    });
                }

                return result;
            }
        }

        [AbpAuthorize(GameHubPermissions.Pages_Company_Employees_Manage)]
        public async Task<CompanyEmployeeDto> InviteAsync(InviteEmployeeInput input)
        {
            User user;
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                user = await _userRepository.FirstOrDefaultAsync(u =>
                    u.UserName == input.EmailOrUserName || u.EmailAddress == input.EmailOrUserName);

                if (user == null)
                {
                    // Try a partial email/username match across tenants.
                    user = await _userRepository.FirstOrDefaultAsync(u =>
                        (u.UserName != null && u.UserName.Contains(input.EmailOrUserName)) ||
                        (u.EmailAddress != null && u.EmailAddress.Contains(input.EmailOrUserName)));
                }
            }

            if (user == null)
            {
                throw new UserFriendlyException($"User '{input.EmailOrUserName}' not found. Ask them to register first.");
            }

            if (user.TenantId.HasValue)
            {
                throw new UserFriendlyException($"User '{input.EmailOrUserName}' is bound to a tenant. Only host accounts can be associated with a company.");
            }

            var role = ParseRole(input.Role);

            var membership = await _tenantUserManager.EnsureMembershipAsync(user.Id, input.TenantId, input.IsDefault);

            await EnsureDeveloperRoleInTenantAsync(input.TenantId, membership.TenantUserId);
            await EnsureTeamMemberAsync(input.TenantId, user.Id, role);

            return new CompanyEmployeeDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                EmailAddress = user.EmailAddress,
                Role = role.ToString(),
                IsDefault = membership.IsDefault,
                JoinedAt = membership.CreationTime,
            };
        }

        [AbpAllowAnonymous]
        public async Task<CompanyEmployeeDto> RegisterAndJoinAsync(JoinCompanyInput input)
        {
            if (string.Equals(input.TenancyName, GameHubConsts.PlayerTenantName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input.TenancyName, Abp.MultiTenancy.AbpTenantBase.DefaultTenantName, StringComparison.OrdinalIgnoreCase))
            {
                throw new UserFriendlyException($"'{input.TenancyName}' is a reserved tenancy name.");
            }

            var tenant = await _tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == input.TenancyName)
                ?? throw new UserFriendlyException($"Company with tenancy name '{input.TenancyName}' not found.");

            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var existing = await _userRepository.FirstOrDefaultAsync(u =>
                    u.UserName == input.UserName || u.EmailAddress == input.EmailAddress);

                if (existing != null)
                {
                    throw new UserFriendlyException("Username or email address is already in use.");
                }
            }

            User hostUser;
            using (AbpSession.Use(null, null))
            using (UnitOfWorkManager.Current.SetTenantId(null))
            {
                hostUser = new User
                {
                    TenantId = null,
                    UserName = input.UserName,
                    Name = input.Name,
                    Surname = input.Surname,
                    EmailAddress = input.EmailAddress,
                    IsActive = true,
                    IsEmailConfirmed = true,
                };

                var createResult = await _userManager.CreateAsync(hostUser, input.Password);
                if (!createResult.Succeeded)
                {
                    throw new UserFriendlyException(string.Join(" ", createResult.Errors.Select(e => e.Description)));
                }

                await EnsureUserRoleAsync(hostUser, "Player");
                await EnsureUserRoleAsync(hostUser, "Developer");
            }

            var membership = await _tenantUserManager.EnsureMembershipAsync(hostUser.Id, tenant.Id, isDefault: false);
            await EnsureDeveloperRoleInTenantAsync(tenant.Id, membership.TenantUserId);
            var role = ParseRole(input.Role);
            await EnsureTeamMemberAsync(tenant.Id, hostUser.Id, role);

            return new CompanyEmployeeDto
            {
                UserId = hostUser.Id,
                UserName = hostUser.UserName,
                EmailAddress = hostUser.EmailAddress,
                Role = role.ToString(),
                IsDefault = membership.IsDefault,
                JoinedAt = membership.CreationTime,
            };
        }

        [AbpAuthorize(GameHubPermissions.Pages_Company_Employees_Manage)]
        public async Task RemoveAsync(RemoveEmployeeInput input)
        {
            await _tenantUserManager.RemoveMembershipAsync(input.UserId, input.TenantId);

            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var team = await _teamRepository.FirstOrDefaultAsync(t => t.TenantId == input.TenantId);
                if (team != null)
                {
                    var teamMember = await _teamMemberRepository.FirstOrDefaultAsync(m => m.TeamId == team.Id && m.UserId == input.UserId);
                    if (teamMember != null)
                    {
                        await _teamMemberRepository.DeleteAsync(teamMember);
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
            }
        }

        [AbpAuthorize(GameHubPermissions.Pages_Company_Employees_Manage)]
        public async Task SetDefaultAsync(SetDefaultEmployeeInput input)
        {
            await _tenantUserManager.SetDefaultAsync(input.UserId, input.TenantId);
        }

        private async Task EnsureDeveloperRoleInTenantAsync(int tenantId, long shadowUserId)
        {
            using (UnitOfWorkManager.Current.SetTenantId(tenantId))
            {
                var role = await _roleManager.FindByNameAsync("Developer");
                if (role == null)
                {
                    role = new Role(tenantId, "Developer", "Developer");
                    (await _roleManager.CreateAsync(role)).CheckErrors();
                }

                var shadowUser = await _userManager.GetUserByIdAsync(shadowUserId);
                if (!await _userManager.IsInRoleAsync(shadowUser, "Developer"))
                {
                    (await _userManager.AddToRoleAsync(shadowUser, "Developer")).CheckErrors();
                }
            }
        }

        private async Task EnsureTeamMemberAsync(int tenantId, long userId, DeveloperTeamRole role)
        {
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var team = await _teamRepository.FirstOrDefaultAsync(t => t.TenantId == tenantId);
                if (team == null)
                {
                    return;
                }

                var existing = await _teamMemberRepository.FirstOrDefaultAsync(m => m.TeamId == team.Id && m.UserId == userId);
                if (existing != null)
                {
                    existing.Role = role;
                    await _teamMemberRepository.UpdateAsync(existing);
                }
                else
                {
                    await _teamMemberRepository.InsertAsync(new DeveloperTeamMember
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        TeamId = team.Id,
                        UserId = userId,
                        Role = role,
                        InvitedAt = Clock.Now,
                        AcceptedAt = Clock.Now,
                    });
                }

                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }

        private async Task EnsureUserRoleAsync(User user, string roleName)
        {
            if (await _userManager.IsInRoleAsync(user, roleName))
            {
                return;
            }

            (await _userManager.AddToRoleAsync(user, roleName)).CheckErrors();
        }

        private static DeveloperTeamRole ParseRole(string role)
        {
            if (Enum.TryParse<DeveloperTeamRole>(role, true, out var parsed))
            {
                return parsed;
            }

            return DeveloperTeamRole.Developer;
        }
    }
}
