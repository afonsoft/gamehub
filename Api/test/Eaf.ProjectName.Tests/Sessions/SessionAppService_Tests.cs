using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Eaf.ProjectName.Tests.Sessions
{
    public class SessionAppService_Tests : ProjectNameTestBase
    {
        public SessionAppService_Tests()
        {
        }

        [MultiTenantFact]
        public async Task Should_Get_Current_User_When_Logged_In_As_Host()
        {
            // Arrange
            LoginAsHostAdmin();

            // Assert
            var currentUser = await GetCurrentUserAsync();
            currentUser.ShouldNotBe(null);
        }

        [Fact]
        public async Task Should_Get_Current_User_And_Tenant_When_Logged_In_As_Tenant()
        {
            // Assert
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();

            currentTenant.ShouldNotBe(null);
            currentUser.ShouldNotBe(null);
            currentUser.Name.ShouldBe("admin");
        }
    }
}