using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Data;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Abp.Timing;
using Abp.UI;
using Eaf.Middleware.Authorization.Roles;
using Eaf.Middleware.MultiTenancy;
using GameHub.Authorization;
using GameHub.Companies.Dto;
using GameHub.Developers;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Companies
{
    [AbpAuthorize(GameHubPermissions.Pages_Companies)]
    public class CompanyAppService : GameHubAppServiceBase, ICompanyAppService
    {
        private readonly IRepository<Tenant, int> _tenantRepository;
        private readonly IRepository<DeveloperTeam, Guid> _teamRepository;
        private readonly RoleManager _roleManager;
        private readonly IPermissionManager _permissionManager;

        public CompanyAppService(
            IRepository<Tenant, int> tenantRepository,
            IRepository<DeveloperTeam, Guid> teamRepository,
            RoleManager roleManager,
            IPermissionManager permissionManager)
        {
            _tenantRepository = tenantRepository;
            _teamRepository = teamRepository;
            _roleManager = roleManager;
            _permissionManager = permissionManager;
        }

        public async Task<PagedResultDto<CompanyDto>> GetAllAsync(PagedAndSortedResultRequestDto input)
        {
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var query = _tenantRepository.GetAll()
                    .Where(t => t.TenancyName != GameHubConsts.PlayerTenantName && t.TenancyName != Abp.MultiTenancy.AbpTenantBase.DefaultTenantName);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(t => t.Name)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .Select(t => new CompanyDto
                    {
                        Id = t.Id,
                        TenancyName = t.TenancyName,
                        Name = t.Name,
                        PrimaryContactEmail = t.Name,
                        Country = string.Empty,
                        IsActive = true,
                        CreationTime = t.CreationTime,
                        EmployeeCount = 0,
                    })
                    .ToListAsync();

                foreach (var item in items)
                {
                    var team = await _teamRepository.FirstOrDefaultAsync(x => x.TenantId == item.Id);
                    if (team != null)
                    {
                        item.Name = team.Name;
                        item.PrimaryContactEmail = team.PrimaryContactEmail;
                        item.Country = team.Country;
                        item.EmployeeCount = await _teamRepository.GetAll()
                            .Where(x => x.TenantId == item.Id)
                            .SelectMany(x => x.Members)
                            .CountAsync();
                    }
                }

                return new PagedResultDto<CompanyDto>(totalCount, items);
            }
        }

        public async Task<CompanyDto> GetAsync(int id)
        {
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var tenant = await _tenantRepository.GetAsync(id);
                return await MapToCompanyDtoAsync(tenant);
            }
        }

        [AbpAllowAnonymous]
        public async Task<CompanyDto> GetByTenancyNameAsync(string tenancyName)
        {
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                if (string.Equals(tenancyName, GameHubConsts.PlayerTenantName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(tenancyName, Abp.MultiTenancy.AbpTenantBase.DefaultTenantName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new UserFriendlyException("This tenant is not a public company.");
                }

                var tenant = await _tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == tenancyName)
                    ?? throw new UserFriendlyException($"Company with tenancy name '{tenancyName}' not found.");

                return await MapToCompanyDtoAsync(tenant);
            }
        }

        [AbpAuthorize(GameHubPermissions.Pages_Companies_Manage)]
        public async Task<CompanyDto> CreateAsync(CreateOrUpdateCompanyInput input)
        {
            var normalizedName = input.TenancyName.ToLowerInvariant();

            if (normalizedName == GameHubConsts.PlayerTenantName.ToLowerInvariant())
            {
                throw new UserFriendlyException($"'{GameHubConsts.PlayerTenantName}' is a reserved tenancy name.");
            }

            if (await _tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == normalizedName) != null)
            {
                throw new UserFriendlyException($"Tenancy name '{input.TenancyName}' is already taken.");
            }

            var tenant = new Tenant(normalizedName, input.Name);
            await _tenantRepository.InsertAsync(tenant);
            await CurrentUnitOfWork.SaveChangesAsync();

            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var team = new DeveloperTeam
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    Name = input.Name,
                    PrimaryContactEmail = input.PrimaryContactEmail,
                    Country = input.Country,
                    CreatedAt = Clock.Now,
                };

                await _teamRepository.InsertAsync(team);
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            await SeedCompanyRolesAsync(tenant.Id);
            return await MapToCompanyDtoAsync(tenant);
        }

        private async Task SeedCompanyRolesAsync(int tenantId)
        {
            using (UnitOfWorkManager.Current.SetTenantId(tenantId))
            {
                await EnsureRoleWithPermissionsAsync(tenantId, "Developer", GameHubPermissions.DeveloperPermissions());
                await EnsureRoleWithPermissionsAsync(tenantId, "Player", GameHubPermissions.PlayerPermissions());
            }
        }

        private async Task EnsureRoleWithPermissionsAsync(int tenantId, string roleName, string[] permissionNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new Role(tenantId, roleName, roleName);
                var result = await _roleManager.CreateAsync(role);
                result.CheckErrors();
            }

            var permissions = new List<Permission>();
            foreach (var name in permissionNames)
            {
                var permission = _permissionManager.GetPermissionOrNull(name);
                if (permission != null)
                {
                    permissions.Add(permission);
                }
            }

            await _roleManager.SetGrantedPermissionsAsync(role, permissions);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Companies_Manage)]
        public async Task<CompanyDto> UpdateAsync(int id, CreateOrUpdateCompanyInput input)
        {
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var tenant = await _tenantRepository.GetAsync(id);
                var normalizedName = input.TenancyName.ToLowerInvariant();

                if (normalizedName == GameHubConsts.PlayerTenantName.ToLowerInvariant())
                {
                    throw new UserFriendlyException($"'{GameHubConsts.PlayerTenantName}' is a reserved tenancy name.");
                }

                var existing = await _tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == normalizedName && t.Id != id);
                if (existing != null)
                {
                    throw new UserFriendlyException($"Tenancy name '{input.TenancyName}' is already taken.");
                }

                tenant.TenancyName = normalizedName;
                tenant.Name = input.Name;

                await _tenantRepository.UpdateAsync(tenant);

                var team = await _teamRepository.FirstOrDefaultAsync(t => t.TenantId == id);
                if (team == null)
                {
                    team = new DeveloperTeam
                    {
                        Id = Guid.NewGuid(),
                        TenantId = id,
                        Name = input.Name,
                        PrimaryContactEmail = input.PrimaryContactEmail,
                        Country = input.Country,
                        CreatedAt = Clock.Now,
                    };
                    await _teamRepository.InsertAsync(team);
                }
                else
                {
                    team.Name = input.Name;
                    team.PrimaryContactEmail = input.PrimaryContactEmail;
                    team.Country = input.Country;
                    await _teamRepository.UpdateAsync(team);
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                return await MapToCompanyDtoAsync(tenant);
            }
        }

        [AbpAuthorize(GameHubPermissions.Pages_Companies_Manage)]
        public async Task DeleteAsync(int id)
        {
            var tenant = await _tenantRepository.GetAsync(id);

            if (tenant.TenancyName == GameHubConsts.PlayerTenantName ||
                tenant.TenancyName == Abp.MultiTenancy.AbpTenantBase.DefaultTenantName)
            {
                throw new UserFriendlyException("Cannot delete reserved tenants.");
            }

            await _tenantRepository.DeleteAsync(id);
        }

        private async Task<CompanyDto> MapToCompanyDtoAsync(Tenant tenant)
        {
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var team = await _teamRepository.FirstOrDefaultAsync(t => t.TenantId == tenant.Id);

                int employeeCount = 0;
                if (team != null)
                {
                    employeeCount = await _teamRepository.GetAll()
                        .Where(t => t.TenantId == tenant.Id)
                        .SelectMany(t => t.Members)
                        .CountAsync();
                }

                return new CompanyDto
                {
                    Id = tenant.Id,
                    TenancyName = tenant.TenancyName,
                    Name = team?.Name ?? tenant.Name,
                    PrimaryContactEmail = team?.PrimaryContactEmail ?? string.Empty,
                    Country = team?.Country ?? string.Empty,
                    IsActive = true,
                    CreationTime = tenant.CreationTime,
                    EmployeeCount = employeeCount,
                };
            }
        }
    }
}
