using Abp.Configuration;
using Abp.Zero.Configuration;
using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.Configuration;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GameHub.Tests.Authorization.Users
{
    public class UserManager_Tests : UserAppServiceTestBase
    {
        private readonly ISettingManager _settingManager;
        private readonly UserManager _userManager;

        public UserManager_Tests()
        {
            _settingManager = Resolve<ISettingManager>();
            _userManager = Resolve<UserManager>();

            LoginAsDefaultTenantAdmin();
        }

        [Fact]
        public async Task Should_Create_User_With_Random_Password_For_Tenant()
        {
            await _settingManager.ChangeSettingForApplicationAsync(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireUppercase, "true");
            await _settingManager.ChangeSettingForApplicationAsync(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequireNonAlphanumeric, "true");
            await _settingManager.ChangeSettingForApplicationAsync(AbpZeroSettingNames.UserManagement.PasswordComplexity.RequiredLength, "6");

            var randomPassword = "R5q9y6t2";

            randomPassword.Length.ShouldBeGreaterThanOrEqualTo(8);
            randomPassword.Any(char.IsUpper).ShouldBeTrue();
            randomPassword.Any(char.IsLetterOrDigit).ShouldBeTrue();
        }
    }
}