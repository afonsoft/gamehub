using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using Abp.UI;
using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.MultiTenancy;
using GameHub.Authorization.Dto;
using GameHub.Developers;
using Microsoft.AspNetCore.Identity;

namespace GameHub.Authorization
{
    /// <summary>
    /// Implements public user registration.
    /// </summary>
    public class RegistrationAppService : GameHubAppServiceBase, IRegistrationAppService
    {
        private readonly IRepository<DeveloperProfile, System.Guid> _developerProfileRepository;
        private readonly IRepository<Tenant, int> _tenantRepository;

        public RegistrationAppService(
            IRepository<DeveloperProfile, System.Guid> developerProfileRepository,
            IRepository<Tenant, int> tenantRepository)
        {
            _developerProfileRepository = developerProfileRepository;
            _tenantRepository = tenantRepository;
        }

        [AbpAllowAnonymous]
        public async Task<RegisterOutput> RegisterAsync(RegisterInput input)
        {
            var defaultTenant = await _tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == AbpTenantBase.DefaultTenantName);
            var tenantId = !input.IsDeveloper && defaultTenant != null ? (int?)defaultTenant.Id : null;

            var user = tenantId.HasValue
                ? await CreatePlayerUserInTenantAsync(input, tenantId.Value)
                : await CreateHostUserAsync(input);

            return new RegisterOutput
            {
                UserId = user.Id,
                UserName = user.UserName,
            };
        }

        private async Task<User> CreatePlayerUserInTenantAsync(RegisterInput input, int tenantId)
        {
            using (AbpSession.Use(tenantId, null))
            using (UnitOfWorkManager.Current.SetTenantId(tenantId))
            {
                var user = new User
                {
                    TenantId = tenantId,
                    UserName = input.UserName,
                    Name = input.Name,
                    Surname = input.Surname,
                    EmailAddress = input.EmailAddress,
                    IsActive = true,
                    IsEmailConfirmed = true,
                };

                var result = await UserManager.CreateAsync(user, input.Password);
                if (!result.Succeeded)
                {
                    throw new UserFriendlyException(string.Join(" ", result.Errors.Select(e => e.Description)));
                }

                await EnsureRoleAsync(user, "Player");
                return user;
            }
        }

        private async Task<User> CreateHostUserAsync(RegisterInput input)
        {
            var user = new User
            {
                TenantId = null,
                UserName = input.UserName,
                Name = input.Name,
                Surname = input.Surname,
                EmailAddress = input.EmailAddress,
                IsActive = true,
                IsEmailConfirmed = true,
            };

            var result = await UserManager.CreateAsync(user, input.Password);
            if (!result.Succeeded)
            {
                throw new UserFriendlyException(string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            await EnsureRoleAsync(user, "Player");

            if (input.IsDeveloper)
            {
                await EnsureRoleAsync(user, "Developer");
                await CreateDeveloperProfileAsync(user);
            }

            return user;
        }

        private async Task EnsureRoleAsync(User user, string roleName)
        {
            if (await UserManager.IsInRoleAsync(user, roleName))
            {
                return;
            }

            var addResult = await UserManager.AddToRoleAsync(user, roleName);
            if (!addResult.Succeeded)
            {
                throw new UserFriendlyException($"Could not assign role {roleName}: {string.Join(" ", addResult.Errors.Select(e => e.Description))}");
            }
        }

        private async Task CreateDeveloperProfileAsync(User user)
        {
            var displayName = $"{user.Name} {user.Surname}".Trim();
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = user.UserName;
            }

            var profile = new DeveloperProfile
            {
                Id = System.Guid.NewGuid(),
                UserId = user.Id,
                TenantId = user.TenantId,
                DisplayName = displayName,
                Status = DeveloperProfileStatus.Active,
            };

            await _developerProfileRepository.InsertAsync(profile);
        }

    }
}
