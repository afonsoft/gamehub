using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.Web.Authentication;
using GameHub.MultiTenancy;
using GameHub.Web.Controllers;
using GameHub.Web.Models.HubAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Controllers
{
    public class HubAuthController_Tests : GameHubTestBase
    {
        private static readonly string TestHostPassword = $"TestPass{System.DateTime.UtcNow.Ticks}A1!";

        public HubAuthController_Tests()
        {
            LoginAsHostAdmin();
        }

        [Fact]
        public async Task Dado_HostAdmin_Quando_ConsultarTenantsDisponiveis_Entao_DeveRetornarDefaultTenant()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.GetAvailableTenants(new AvailableTenantsModel
            {
                UserNameOrEmailAddress = Abp.Authorization.Users.AbpUserBase.AdminUserName,
                Password = "123qwe",
            });

            // Assert
            var ok = result.ShouldBeAssignableTo<OkObjectResult>();
            var tenants = ok.Value.ShouldBeAssignableTo<List<AvailableTenantResult>>();
            tenants.ShouldContain(t => t.TenancyName == Abp.MultiTenancy.AbpTenantBase.DefaultTenantName);
        }

        [Fact]
        public async Task Dado_HostAdmin_Quando_SelecionarDefaultTenant_Entao_DeveRetornarAccessToken()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var availableResult = await controller.GetAvailableTenants(new AvailableTenantsModel
            {
                UserNameOrEmailAddress = Abp.Authorization.Users.AbpUserBase.AdminUserName,
                Password = "123qwe",
            });

            var ok = availableResult.ShouldBeAssignableTo<OkObjectResult>();
            var tenants = ok.Value.ShouldBeAssignableTo<List<AvailableTenantResult>>();
            var defaultTenant = tenants.Find(t => t.TenancyName == Abp.MultiTenancy.AbpTenantBase.DefaultTenantName);
            defaultTenant.ShouldNotBeNull();

            var selectResult = await controller.SelectTenant(new SelectTenantModel
            {
                UserNameOrEmailAddress = Abp.Authorization.Users.AbpUserBase.AdminUserName,
                Password = "123qwe",
                TenantId = defaultTenant.TenantId,
            });

            // Assert
            var selectOk = selectResult.ShouldBeAssignableTo<OkObjectResult>();
            var tokenResult = selectOk.Value.ShouldBeAssignableTo<SelectTenantResult>();
            tokenResult.AccessToken.ShouldNotBeNullOrEmpty();
            tokenResult.TenantId.ShouldBe(defaultTenant.TenantId);
        }

        [Fact]
        public async Task Dado_CredenciaisInvalidas_Quando_ConsultarTenants_Entao_DeveLancarExcecaoAmigavel()
        {
            // Arrange
            var controller = CreateController();

            // Act & Assert
            await Should.ThrowAsync<Abp.UI.UserFriendlyException>(async () =>
                await controller.GetAvailableTenants(new AvailableTenantsModel
                {
                    UserNameOrEmailAddress = "unknown",
                    Password = "wrong",
                }));
        }

        [Fact]
        public async Task Dado_UsuarioHostSemTenants_Quando_ConsultarTenantsDisponiveis_Entao_DeveRetornarListaVazia()
        {
            // Arrange
            var user = await CriarUsuarioHostAsync("hostnotenant");
            var controller = CreateController();

            // Act
            var result = await controller.GetAvailableTenants(new AvailableTenantsModel
            {
                UserNameOrEmailAddress = user.UserName,
                Password = TestHostPassword,
            });

            // Assert
            var ok = result.ShouldBeAssignableTo<OkObjectResult>();
            var tenants = ok.Value.ShouldBeAssignableTo<List<AvailableTenantResult>>();
            tenants.ShouldBeEmpty();
        }

        private async Task<User> CriarUsuarioHostAsync(string userName)
        {
            var userManager = Resolve<UserManager>();
            var unitOfWorkManager = Resolve<IUnitOfWorkManager>();

            using var uow = unitOfWorkManager.Begin();
            var user = new User
            {
                TenantId = null,
                UserName = userName,
                Name = userName,
                Surname = "Test",
                EmailAddress = $"{userName}@gamehub.local",
                IsEmailConfirmed = true,
                IsActive = true,
                Password = new PasswordHasher<User>().HashPassword(null, TestHostPassword),
            };

            (await userManager.CreateAsync(user)).CheckErrors();
            await uow.CompleteAsync();

            return user;
        }

        private HubAuthController CreateController()
        {
            var tokenAuthenticationService = Resolve<ITokenAuthenticationService>();
            var userManager = Resolve<UserManager>();
            var membershipRepository = Resolve<IRepository<global::GameHub.MultiTenancy.UserTenantMembership, long>>();
            var tenantRepository = Resolve<IRepository<global::Eaf.Middleware.MultiTenancy.Tenant, int>>();
            var userRepository = Resolve<IRepository<User, long>>();
            var unitOfWorkManager = Resolve<IUnitOfWorkManager>();

            return new HubAuthController(
                tokenAuthenticationService,
                userManager,
                membershipRepository,
                tenantRepository,
                userRepository,
                unitOfWorkManager);
        }
    }
}
