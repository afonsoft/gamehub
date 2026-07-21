using Eaf.Middleware.Authorization.Roles;
using Eaf.Middleware.Authorization.Roles.Dto;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace GameHub.Tests.Authorization.Roles
{
    // ReSharper disable once InconsistentNaming
    public class RoleAppService_Tests : ProjectNameTestBase
    {
        private readonly IRoleAppService _roleAppService;

        public RoleAppService_Tests()
        {
            _roleAppService = Resolve<IRoleAppService>();
        }

        [MultiTenantFact]
        public async Task Should_Get_Roles_For_Host()
        {
            LoginAsHostAdmin();

            //Act
            var output = await _roleAppService.GetRoles(new GetRolesInput());

            //Assert
            output.Items.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Should_Get_Roles_For_Tenant()
        {
            //Act
            var output = await _roleAppService.GetRoles(new GetRolesInput());

            //Assert
            output.Items.Count.ShouldBe(1);
        }
    }
}