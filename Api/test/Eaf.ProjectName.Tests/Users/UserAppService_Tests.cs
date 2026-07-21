using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.Authorization.Users.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Eaf.ProjectName.Tests.Users
{
    public class UserAppService_Tests : ProjectNameTestBase
    {
        private readonly IUserAppService _userAppService;

        public UserAppService_Tests()
        {
            _userAppService = Resolve<IUserAppService>();
        }

        [Fact]
        public async Task GetUsers_Test()
        {
            // Act
            var output = await _userAppService.GetUsers(new GetUsersInput { MaxResultCount = 20, SkipCount = 0 });

            // Assert
            output.Items.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task CreateUser_Test()
        {
            // Act
            await _userAppService.CreateOrUpdateUser(
                new CreateOrUpdateUserInput
                {
                    SendActivationEmail = false,
                    SetRandomPassword = false,
                    AssignedRoleNames = ["Admin"],
                    User = new UserEditDto
                    {
                        EmailAddress = "john@volosoft.com",
                        IsActive = true,
                        Name = "John",
                        Surname = "Nash",
                        Password = "123qwe",
                        UserName = "john.nash",
                        IsLockoutEnabled = false,
                        PhoneNumber = "551198536845",
                        ShouldChangePasswordOnNextLogin = false
                    }
                });

            await UsingDbContextAsync(async context =>
            {
                var johnNashUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "john.nash");
                johnNashUser.ShouldNotBeNull();
            });
        }
    }
}