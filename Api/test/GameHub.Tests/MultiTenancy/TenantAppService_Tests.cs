using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Abp.Zero.Configuration;
using GameHub;
using Eaf.Middleware.MultiTenancy;
using Eaf.Middleware.MultiTenancy.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;

namespace GameHub.Tests.MultiTenancy
{
    // ReSharper disable once InconsistentNaming
    public class TenantAppService_Tests : GameHubTestBase
    {
        private readonly ITenantAppService _tenantAppService;

        public TenantAppService_Tests()
        {
            LoginAsHostAdmin();

            _tenantAppService = Resolve<ITenantAppService>();
        }

        [MultiTenantFact]
        public async Task GetTenants_Test()
        {
            //Act
            var output = await _tenantAppService.GetTenants(new GetTenantsInput());

            //Assert
            output.TotalCount.ShouldBe(2);
            output.Items.Count.ShouldBe(2);
            output.Items.Select(t => t.TenancyName).ShouldContain(AbpTenantBase.DefaultTenantName);
            output.Items.Select(t => t.TenancyName).ShouldContain(GameHubConsts.PlayerTenantName);
        }
    }
}