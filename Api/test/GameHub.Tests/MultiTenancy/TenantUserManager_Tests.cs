using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.MultiTenancy;
using GameHub.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace GameHub.Tests.MultiTenancy
{
    public class TenantUserManager_Tests : GameHubTestBase
    {
        private readonly ITenantUserManager _tenantUserManager;
        private readonly IRepository<UserTenantMembership, long> _membershipRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Tenant, int> _tenantRepository;
        private readonly UserManager _userManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public TenantUserManager_Tests()
        {
            _tenantUserManager = Resolve<ITenantUserManager>();
            _membershipRepository = Resolve<IRepository<UserTenantMembership, long>>();
            _userRepository = Resolve<IRepository<User, long>>();
            _tenantRepository = Resolve<IRepository<Tenant, int>>();
            _userManager = Resolve<UserManager>();
            _unitOfWorkManager = Resolve<IUnitOfWorkManager>();

            LoginAsHostAdmin();
        }

        [Fact]
        public async Task Dado_UsuarioHost_Quando_AssociarAUmTenant_Entao_DeveCriarShadowUserComMesmasCredenciais()
        {
            // Arrange
            var hostUser = await CriarUsuarioHostAsync("dev1");
            var tenant = await _tenantRepository.GetAsync(1);

            // Act & Assert
            UserTenantMembership membership;
            using (var uow = _unitOfWorkManager.Begin())
            {
                membership = await _tenantUserManager.EnsureMembershipAsync(hostUser.Id, tenant.Id, true);
                await uow.CompleteAsync();
            }

            membership.ShouldNotBeNull();
            membership.UserId.ShouldBe(hostUser.Id);
            membership.TenantId.ShouldBe(tenant.Id);
            membership.IsDefault.ShouldBeTrue();

            using (var uow = _unitOfWorkManager.Begin())
            using (_unitOfWorkManager.Current.SetTenantId(tenant.Id))
            {
                var shadowUser = await _userRepository.GetAsync(membership.TenantUserId);
                shadowUser.TenantId.ShouldBe(tenant.Id);
                shadowUser.UserName.ShouldBe(hostUser.UserName);

                var passwordValid = await _userManager.CheckPasswordAsync(shadowUser, "123qwe");
                passwordValid.ShouldBeTrue();

                await uow.CompleteAsync();
            }
        }

        [Fact]
        public async Task Dado_UsuarioComMultiplosTenants_Quando_DefinirDefault_Entao_SomenteUmDeveSerDefault()
        {
            // Arrange
            var hostUser = await CriarUsuarioHostAsync("dev2");
            var defaultTenant = await _tenantRepository.GetAsync(1);
            var secondTenant = await CriarTenantAsync("acme");

            using (var uow = _unitOfWorkManager.Begin())
            {
                await _tenantUserManager.EnsureMembershipAsync(hostUser.Id, defaultTenant.Id, true);
                await _tenantUserManager.EnsureMembershipAsync(hostUser.Id, secondTenant.Id, false);
                await _tenantUserManager.EnsureMembershipAsync(hostUser.Id, secondTenant.Id, true);
                await uow.CompleteAsync();
            }

            // Assert
            using (var uow = _unitOfWorkManager.Begin())
            {
                var memberships = await _membershipRepository.GetAllListAsync(m => m.UserId == hostUser.Id);
                memberships.Count.ShouldBe(2);
                memberships.Count(m => m.IsDefault).ShouldBe(1);
                memberships.ShouldContain(m => m.TenantId == secondTenant.Id && m.IsDefault);
                await uow.CompleteAsync();
            }
        }

        [Fact]
        public async Task Dado_UsuarioTenant_Quando_TentarAssociar_Entao_DeveLancarExcecao()
        {
            // Arrange
            var tenant = await _tenantRepository.GetAsync(1);
            User tenantUser;
            using (var uow = _unitOfWorkManager.Begin())
            using (_unitOfWorkManager.Current.SetTenantId(tenant.Id))
            {
                tenantUser = await _userRepository.FirstOrDefaultAsync(u => u.UserName == AbpUserBase.AdminUserName && u.TenantId == tenant.Id);
                await uow.CompleteAsync();
            }

            tenantUser.ShouldNotBeNull();

            // Act & Assert
            await Should.ThrowAsync<Abp.UI.UserFriendlyException>(async () =>
            {
                using var uow = _unitOfWorkManager.Begin();
                await _tenantUserManager.EnsureMembershipAsync(tenantUser.Id, tenant.Id, true);
                await uow.CompleteAsync();
            });
        }

        [Fact]
        public async Task Dado_UsuarioComMembership_Quando_Remover_Entao_ShadowUserDeveSerExcluido()
        {
            // Arrange
            var hostUser = await CriarUsuarioHostAsync("dev3");
            var tenant = await _tenantRepository.GetAsync(1);
            long shadowUserId;

            using (var uow = _unitOfWorkManager.Begin())
            {
                var membership = await _tenantUserManager.EnsureMembershipAsync(hostUser.Id, tenant.Id, true);
                shadowUserId = membership.TenantUserId;
                await uow.CompleteAsync();
            }

            // Act
            using (var uow = _unitOfWorkManager.Begin())
            {
                await _tenantUserManager.RemoveMembershipAsync(hostUser.Id, tenant.Id);
                await uow.CompleteAsync();
            }

            // Assert
            using (var uow = _unitOfWorkManager.Begin())
            {
                var removedMembership = await _membershipRepository.FirstOrDefaultAsync(m => m.UserId == hostUser.Id && m.TenantId == tenant.Id);
                removedMembership.ShouldBeNull();

                using (_unitOfWorkManager.Current.SetTenantId(tenant.Id))
                {
                    var shadowUser = await _userRepository.FirstOrDefaultAsync(u => u.Id == shadowUserId);
                    shadowUser.ShouldBeNull();
                }

                await uow.CompleteAsync();
            }
        }

        private async Task<User> CriarUsuarioHostAsync(string userName)
        {
            User user;
            using (var uow = _unitOfWorkManager.Begin())
            {
                user = new User
                {
                    TenantId = null,
                    UserName = userName,
                    Name = userName,
                    Surname = "Test",
                    EmailAddress = $"{userName}@gamehub.local",
                    IsEmailConfirmed = true,
                    IsActive = true,
                    Password = new PasswordHasher<User>().HashPassword(null, "123qwe"),
                };

                (await _userManager.CreateAsync(user)).CheckErrors();
                await uow.CompleteAsync();
            }

            return user;
        }

        private async Task<Tenant> CriarTenantAsync(string tenancyName)
        {
            Tenant tenant;
            using (var uow = _unitOfWorkManager.Begin())
            {
                tenant = new Tenant(tenancyName, tenancyName);
                await _tenantRepository.InsertAsync(tenant);
                await uow.CompleteAsync();
            }

            return tenant;
        }
    }
}
