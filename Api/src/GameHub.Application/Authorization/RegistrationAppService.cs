using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Editions;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Abp.UI;
using Eaf.Middleware.Authorization.Roles;
using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.MultiTenancy;
using GameHub.Authorization.Dto;
using GameHub.Developers;
using GameHub.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using static Eaf.Middleware.Authorization.Roles.StaticRoleNames;

namespace GameHub.Authorization
{
    /// <summary>
    /// Implements public user registration supporting player, developer and tenant registration flows.
    /// </summary>
    public class RegistrationAppService : GameHubAppServiceBase, IRegistrationAppService
    {
        private readonly IRepository<DeveloperProfile, System.Guid> _developerProfileRepository;
        private readonly IRepository<Tenant, int> _tenantRepository;
        private readonly IRepository<GameHub.MultiTenancy.TenantJoinRequest, long> _tenantJoinRequestRepository;
        private readonly IRepository<GameHub.MultiTenancy.UserTenantMembership, long> _userTenantMembershipRepository;
        private readonly IRepository<Edition> _editionRepository;
        private readonly RoleManager _roleManager;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public RegistrationAppService(
            IRepository<DeveloperProfile, System.Guid> developerProfileRepository,
            IRepository<Tenant, int> tenantRepository,
            IRepository<GameHub.MultiTenancy.TenantJoinRequest, long> tenantJoinRequestRepository,
            IRepository<GameHub.MultiTenancy.UserTenantMembership, long> userTenantMembershipRepository,
            IRepository<Edition> editionRepository,
            RoleManager roleManager,
            IPasswordHasher<User> passwordHasher,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _developerProfileRepository = developerProfileRepository;
            _tenantRepository = tenantRepository;
            _tenantJoinRequestRepository = tenantJoinRequestRepository;
            _userTenantMembershipRepository = userTenantMembershipRepository;
            _editionRepository = editionRepository;
            _roleManager = roleManager;
            _passwordHasher = passwordHasher;
            _unitOfWorkManager = unitOfWorkManager;
        }

        [AbpAllowAnonymous]
        public async Task<RegisterOutput> RegisterAsync(RegisterInput input)
        {
            if (input.TenantSelectionMode == TenantSelectionModes.CreateNew)
            {
                var (tenantId, adminUserId) = await CreateTenantWithAdminUserAsync(input);
                var tenant = await _tenantRepository.GetAsync(tenantId);
                return new RegisterOutput
                {
                    UserId = adminUserId,
                    UserName = input.UserName,
                    TenancyName = tenant.TenancyName,
                    TenantId = tenantId,
                    CanLogin = true,
                };
            }

            if (input.TenantSelectionMode == TenantSelectionModes.JoinExisting)
            {
                var (joinUser, tenantId) = await CreateJoinTenantUserAsync(input);
                return new RegisterOutput
                {
                    UserId = joinUser.Id,
                    UserName = joinUser.UserName,
                    TenantId = tenantId,
                    CanLogin = false,
                };
            }

            var playerTenant = await _tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == GameHubConsts.PlayerTenantName);
            var fallbackTenantId = !input.IsDeveloper && playerTenant != null ? (int?)playerTenant.Id : null;

            var user = fallbackTenantId.HasValue
                ? await CreatePlayerUserInTenantAsync(input, fallbackTenantId.Value)
                : await CreateDeveloperUserAsync(input);

            return new RegisterOutput
            {
                UserId = user.Id,
                UserName = user.UserName,
                CanLogin = true,
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

        private async Task<User> CreateDeveloperUserAsync(RegisterInput input)
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

        private async Task<(int TenantId, long AdminUserId)> CreateTenantWithAdminUserAsync(RegisterInput input)
        {
            if (string.IsNullOrWhiteSpace(input.NewTenantName))
            {
                throw new UserFriendlyException(L("TenantNameIsRequired"));
            }

            var tenancyName = input.NewTenantName.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("_", "-")
                .Replace(".", "-");

            tenancyName = string.Join("-", tenancyName.Split('-').Where(s => !string.IsNullOrEmpty(s)));
            if (string.IsNullOrWhiteSpace(tenancyName))
            {
                throw new UserFriendlyException(L("TenantNameIsInvalid"));
            }

            using (var uow = _unitOfWorkManager.Begin())
            {
                var edition = await _editionRepository.FirstOrDefaultAsync(e => e.Name == "Free")
                    ?? new Edition { Name = "Free", DisplayName = "Free" };

                if (edition.Id == 0)
                {
                    await _editionRepository.InsertAsync(edition);
                    await _unitOfWorkManager.Current.SaveChangesAsync();
                }

                var tenant = new Tenant(tenancyName, input.NewTenantName)
                {
                    IsActive = true,
                    EditionId = edition.Id,
                };
                await _tenantRepository.InsertAsync(tenant);
                await _unitOfWorkManager.Current.SaveChangesAsync();

                var tenantId = tenant.Id;

                var hostUser = new User
                {
                    TenantId = null,
                    UserName = input.UserName,
                    Name = input.Name,
                    Surname = input.Surname,
                    EmailAddress = input.EmailAddress,
                    IsActive = true,
                    IsEmailConfirmed = true,
                };

                var hostResult = await UserManager.CreateAsync(hostUser, input.Password);
                if (!hostResult.Succeeded)
                {
                    throw new UserFriendlyException(string.Join(" ", hostResult.Errors.Select(e => e.Description)));
                }

                await _unitOfWorkManager.Current.SaveChangesAsync();

                using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                {
                    (await _roleManager.CreateStaticRoles(tenantId)).CheckErrors();
                    await _unitOfWorkManager.Current.SaveChangesAsync();

                    var adminRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.Admin);
                    await _roleManager.GrantAllPermissionsAsync(adminRole);

                    var userRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.User);

                    var developRole = new Role(tenantId, GameHubRoleNames.Develop, GameHubRoleNames.Develop)
                    {
                        IsStatic = false,
                        IsDefault = false,
                    };
                    (await _roleManager.CreateAsync(developRole)).CheckErrors();

                    var adminUser = new User
                    {
                        TenantId = tenantId,
                        UserName = "admin",
                        Name = input.Name,
                        Surname = input.Surname,
                        EmailAddress = input.EmailAddress,
                        IsActive = true,
                        IsEmailConfirmed = true,
                    };

                    await UserManager.InitializeOptionsAsync(tenantId);
                    foreach (var validator in UserManager.PasswordValidators)
                    {
                        (await validator.ValidateAsync(UserManager, adminUser, input.Password)).CheckErrors();
                    }

                    adminUser.Password = _passwordHasher.HashPassword(adminUser, input.Password);
                    (await UserManager.CreateAsync(adminUser)).CheckErrors();
                    await _unitOfWorkManager.Current.SaveChangesAsync();

                    (await UserManager.AddToRoleAsync(adminUser, adminRole.Name)).CheckErrors();
                    (await UserManager.AddToRoleAsync(adminUser, userRole.Name)).CheckErrors();
                    (await UserManager.AddToRoleAsync(adminUser, developRole.Name)).CheckErrors();

                    var membership = new GameHub.MultiTenancy.UserTenantMembership
                    {
                        UserId = hostUser.Id,
                        TenantId = tenantId,
                        TenantUserId = adminUser.Id,
                        IsDefault = true,
                    };
                    await _userTenantMembershipRepository.InsertAsync(membership);

                    await _unitOfWorkManager.Current.SaveChangesAsync();
                    await uow.CompleteAsync();

                    return (tenantId, hostUser.Id);
                }
            }
        }

        private async Task EnsureDevelopRoleAsync(int tenantId)
        {
            using (_unitOfWorkManager.Current.SetTenantId(tenantId))
            {
                var existingRole = await _roleManager.FindByNameAsync(GameHubRoleNames.Develop);
                if (existingRole == null)
                {
                    var role = new Role(tenantId, GameHubRoleNames.Develop, GameHubRoleNames.Develop)
                    {
                        IsStatic = false,
                        IsDefault = false,
                    };
                    var result = await _roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                    {
                        throw new UserFriendlyException($"Could not create {GameHubRoleNames.Develop} role: {string.Join(" ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private async Task<(User HostUser, int TenantId)> CreateJoinTenantUserAsync(RegisterInput input)
        {
            var hostUser = await CreateHostUserAsync(input);
            var tenantId = input.ExistingTenantId.Value;

            using (_unitOfWorkManager.Current.SetTenantId(tenantId))
            {
                await UserManager.InitializeOptionsAsync(tenantId);

                var shadowUser = new User
                {
                    TenantId = tenantId,
                    UserName = hostUser.UserName,
                    Name = hostUser.Name,
                    Surname = hostUser.Surname,
                    EmailAddress = hostUser.EmailAddress,
                    IsActive = false,
                    IsEmailConfirmed = true,
                    IsLockoutEnabled = false,
                };
                shadowUser.SetNormalizedNames();

                shadowUser.Password = _passwordHasher.HashPassword(shadowUser, Guid.NewGuid().ToString("N"));
                (await UserManager.CreateAsync(shadowUser)).CheckErrors();
                await _unitOfWorkManager.Current.SaveChangesAsync();

                var userRole = await _roleManager.FindByNameAsync(Tenants.User);
                if (userRole != null)
                {
                    (await UserManager.AddToRoleAsync(shadowUser, userRole.Name)).CheckErrors();
                }

                await _userTenantMembershipRepository.InsertAsync(new GameHub.MultiTenancy.UserTenantMembership
                {
                    UserId = hostUser.Id,
                    TenantId = tenantId,
                    TenantUserId = shadowUser.Id,
                    IsDefault = false,
                });

                await _unitOfWorkManager.Current.SaveChangesAsync();
            }

            await CreateJoinRequestAsync(hostUser.Id, tenantId, input.JoinRequestMessage);
            return (hostUser, tenantId);
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

            return user;
        }

        private async Task CreateJoinRequestAsync(long userId, int tenantId, string message)
        {
            var existing = await _tenantJoinRequestRepository.FirstOrDefaultAsync(r =>
                r.UserId == userId && r.TenantId == tenantId && r.Status == GameHub.MultiTenancy.TenantJoinRequestStatus.Pending);
            if (existing != null)
            {
                throw new UserFriendlyException(L("TenantJoinRequestAlreadyPending"));
            }

            await _tenantJoinRequestRepository.InsertAsync(new GameHub.MultiTenancy.TenantJoinRequest
            {
                UserId = userId,
                TenantId = tenantId,
                Status = GameHub.MultiTenancy.TenantJoinRequestStatus.Pending,
                Message = message,
            });
        }

        public static class GameHubRoleNames
        {
            public const string Develop = "Develop";
        }
    }
}
