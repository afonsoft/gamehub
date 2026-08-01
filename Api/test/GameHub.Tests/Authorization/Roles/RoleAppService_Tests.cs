using Eaf.Middleware.Authorization.Roles;
using Eaf.Middleware.Authorization.Roles.Dto;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace GameHub.Tests.Authorization.Roles
{
    // ReSharper disable once InconsistentNaming
    public class RoleAppService_Tests : GameHubTestBase
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

            //Assert: Admin, Developer, Player
            output.Items.Count.ShouldBe(3);
        }

        [Fact]
        public async Task Should_Get_Roles_For_Tenant()
        {
            //Act
            var output = await _roleAppService.GetRoles(new GetRolesInput());

            //Assert: Admin, User, Moderator, Developer, Player
            output.Items.Count.ShouldBe(5);
        }
    }
}