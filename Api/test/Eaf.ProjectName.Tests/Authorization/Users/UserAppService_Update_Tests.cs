using Abp.Authorization.Users;
using Abp.UI;
using Eaf.Middleware.Authorization.Roles;
using Eaf.Middleware.Authorization.Users.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace Eaf.ProjectName.Tests.Authorization.Users
{
    // ReSharper disable once InconsistentNaming
    public class UserAppService_Update_Tests : UserAppServiceTestBase
    {
        [Fact]
        public async Task Should_Not_Update_User_With_Duplicate_Username_Or_EmailAddress()
        {
            //Arrange - Configurar culture para pt-BR
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");

            CreateTestUsers();
            var jnashUser = await GetUserByUserNameOrNullAsync("jnash");

            //Act

            //Try to update with existing username
            var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                await UserAppService.CreateOrUpdateUser(
                    new CreateOrUpdateUserInput
                    {
                        User = new UserEditDto
                        {
                            Id = jnashUser.Id,
                            EmailAddress = "jnsh2000@testdomain.com",
                            Name = "John",
                            Surname = "Nash",
                            UserName = "adams_d", //Changed user name to an existing user
                            Password = "123qwE*"
                        },
                        AssignedRoleNames = new string[0]
                    }));

            exception.Message.ShouldContain("Uma falha desconhecida ocorreu.");
        }

        [MultiTenantFact]
        public async Task Should_Remove_From_Role()
        {
            LoginAsHostAdmin();

            //Arrange
            var adminUser = await GetUserByUserNameOrNullAsync(AbpUserBase.AdminUserName);
            await UsingDbContextAsync(async context =>
            {
                var roleCount = await context.UserRoles.CountAsync(ur => ur.UserId == adminUser.Id);
                roleCount.ShouldBeGreaterThan(0); //There should be 1 role at least
            });

            //Act
            await UserAppService.CreateOrUpdateUser(
                new CreateOrUpdateUserInput
                {
                    User = new UserEditDto //Not changing user properties
                    {
                        Id = adminUser.Id,
                        EmailAddress = adminUser.EmailAddress,
                        Name = adminUser.Name,
                        Surname = adminUser.Surname,
                        UserName = adminUser.UserName,
                        Password = null
                    },
                    AssignedRoleNames = new[] { StaticRoleNames.Host.Admin } //Just deleting all roles expect admin
                });

            //Assert
            await UsingDbContextAsync(async context =>
            {
                var roleCount = await context.UserRoles.CountAsync(ur => ur.UserId == adminUser.Id);
                roleCount.ShouldBe(1);
            });
        }

        protected Role CreateRole(string roleName)
        {
            return UsingDbContext(context => context.Roles.Add(new Role(AbpSession.TenantId, roleName, roleName)).Entity);
        }
    }
}