using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Eaf.Middleware.Authorization.Users;
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

        public RegistrationAppService(IRepository<DeveloperProfile, System.Guid> developerProfileRepository)
        {
            _developerProfileRepository = developerProfileRepository;
        }

        [AbpAllowAnonymous]
        public async Task<RegisterOutput> RegisterAsync(RegisterInput input)
        {
            var user = new User
            {
                TenantId = AbpSession.TenantId,
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

            return new RegisterOutput
            {
                UserId = user.Id,
                UserName = user.UserName,
            };
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
