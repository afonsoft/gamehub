using Abp.Application.Services.Dto;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Eaf.ProjectName.Tests.Authorization.Users
{
    // ReSharper disable once InconsistentNaming
    public class UserAppService_Delete_Tests : UserAppServiceTestBase
    {
        [Fact]
        public async Task Should_Delete_User()
        {
            //Arrange
            CreateTestUsers();

            var user = await GetUserByUserNameOrNullAsync("artdent");

            if (user == null)
            {
                user.ShouldBe(null);
                return;
            }

            //Act
            await UserAppService.DeleteUser(new EntityDto<long>(user.Id));

            //Assert
            user = await GetUserByUserNameOrNullAsync("artdent");
            user.IsDeleted.ShouldBe(true);
        }
    }
}