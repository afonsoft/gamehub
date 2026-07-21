using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Abp.Zero.Configuration;
using Eaf.Middleware.MultiTenancy;
using Eaf.Middleware.MultiTenancy.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;

namespace Eaf.ProjectName.Tests.MultiTenancy
{
    // ReSharper disable once InconsistentNaming
    public class TenantAppService_Tests : ProjectNameTestBase
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
            output.TotalCount.ShouldBe(1);
            output.Items.Count.ShouldBe(1);
            output.Items[0].TenancyName.ShouldBe(AbpTenantBase.DefaultTenantName);
        }
    }
}