using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.MultiTenancy;
using GameHub.MultiTenancy;
using GameHub.MultiTenancy.Dto;
using Microsoft.AspNetCore.Identity;
using Shouldly;
using Xunit;

namespace GameHub.Tests.MultiTenancy
{
    public class UserTenantAssociationAppService_Tests : GameHubTestBase
    {
        private readonly IUserTenantAssociationAppService _appService;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Tenant, int> _tenantRepository;
        private readonly UserManager _userManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public UserTenantAssociationAppService_Tests()
        {
            _appService = Resolve<IUserTenantAssociationAppService>();
            _userRepository = Resolve<IRepository<User, long>>();
            _tenantRepository = Resolve<IRepository<Tenant, int>>();
            _userManager = Resolve<UserManager>();
            _unitOfWorkManager = Resolve<IUnitOfWorkManager>();

            LoginAsHostAdmin();
        }

        [Fact]
        public async Task Dado_UsuarioHost_Quando_AssociarViaAppService_Entao_DeveRetornarMembershipComTenantName()
        {
            // Arrange
            var hostUser = await CriarUsuarioHostAsync("assoc1");
            var tenant = await _tenantRepository.GetAsync(1);

            // Act
            UserTenantMembershipDto result;
            using (var uow = _unitOfWorkManager.Begin())
            {
                result = await _appService.AssociateAsync(new AssociateUserToTenantInput
                {
                    UserId = hostUser.Id,
                    TenantId = tenant.Id,
                    IsDefault = true,
                });
                await uow.CompleteAsync();
            }

            // Assert
            result.ShouldNotBeNull();
            result.UserId.ShouldBe(hostUser.Id);
            result.TenantId.ShouldBe(tenant.Id);
            result.TenantName.ShouldBe(tenant.Name);
            result.TenantTenancyName.ShouldBe(tenant.TenancyName);
            result.IsDefault.ShouldBeTrue();
        }

        [Fact]
        public async Task Dado_UsuarioComMembership_Quando_ListarTenants_Entao_DeveRetornarLista()
        {
            // Arrange
            var hostUser = await CriarUsuarioHostAsync("assoc2");
            var tenant = await _tenantRepository.GetAsync(1);

            using (var uow = _unitOfWorkManager.Begin())
            {
                await _appService.AssociateAsync(new AssociateUserToTenantInput
                {
                    UserId = hostUser.Id,
                    TenantId = tenant.Id,
                    IsDefault = true,
                });
                await uow.CompleteAsync();
            }

            // Act
            System.Collections.Generic.List<UserTenantMembershipDto> memberships;
            using (var uow = _unitOfWorkManager.Begin())
            {
                memberships = await _appService.GetUserMembershipsAsync(new GetUserTenantMembershipsInput
                {
                    UserId = hostUser.Id,
                });
                await uow.CompleteAsync();
            }

            // Assert
            memberships.Count.ShouldBe(1);
            memberships.First().TenantId.ShouldBe(tenant.Id);
        }

        [Fact]
        public async Task Dado_UsuarioComMultiplosTenants_Quando_DefinirDefault_Entao_SomenteUmDefault()
        {
            // Arrange
            var hostUser = await CriarUsuarioHostAsync("assoc3");
            var defaultTenant = await _tenantRepository.GetAsync(1);
            var secondTenant = await CriarTenantAsync("gamecorp");

            using (var uow = _unitOfWorkManager.Begin())
            {
                await _appService.AssociateAsync(new AssociateUserToTenantInput
                {
                    UserId = hostUser.Id,
                    TenantId = defaultTenant.Id,
                    IsDefault = true,
                });

                await _appService.AssociateAsync(new AssociateUserToTenantInput
                {
                    UserId = hostUser.Id,
                    TenantId = secondTenant.Id,
                    IsDefault = false,
                });

                await _appService.SetDefaultAsync(new SetDefaultTenantInput
                {
                    UserId = hostUser.Id,
                    TenantId = secondTenant.Id,
                });

                await uow.CompleteAsync();
            }

            // Assert
            using (var uow = _unitOfWorkManager.Begin())
            {
                var memberships = await _appService.GetUserMembershipsAsync(new GetUserTenantMembershipsInput
                {
                    UserId = hostUser.Id,
                });

                memberships.Count(m => m.IsDefault).ShouldBe(1);
                memberships.ShouldContain(m => m.TenantId == secondTenant.Id && m.IsDefault);
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
