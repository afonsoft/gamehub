using Abp.Application.Services.Dto;
using Eaf.Middleware.Authorization.Roles;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;

namespace Eaf.ProjectName.Tests.Authorization.Users
{
    // ReSharper disable once InconsistentNaming
    public class UserAppService_GetUserForEdit_Tests : UserAppServiceTestBase
    {
        [MultiTenantFact]
        public async Task Should_Work_For_NonExisting_User()
        {
            //Arrange
            LoginAsHostAdmin();

            //Act
            var output = await UserAppService.GetUserForEdit(new NullableIdDto<long>());

            //Assert
            output.User.Id.ShouldBe(null);
            output.User.Name.ShouldBe(null);
            output.User.Password.ShouldBe(null);

            output.Roles.Length.ShouldBe(1);
            output.Roles.Any(r => r.RoleName == StaticRoleNames.Host.Admin).ShouldBe(true);
            output.Roles.Single(r => r.RoleName == StaticRoleNames.Host.Admin).IsAssigned.ShouldBe(true);
        }

        protected Role CreateRole(string roleName)
        {
            return UsingDbContext(context => context.Roles.Add(new Role(AbpSession.TenantId, roleName, roleName)).Entity);
        }
    }
}