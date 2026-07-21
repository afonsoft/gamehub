using Eaf.Controllers;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Eaf.ProjectName.Web.Tests.Controllers
{
    public class HomeController_Tests : ProjectNameWebTestBase
    {
        [Fact(Skip = "Skipped in CI: depends on external configuration/database")]
        public async Task About_Test()
        {
            //Act
            var response = await GetResponseAsStringAsync("/api/services/app" +
                GetUrl<AboutController>(nameof(AboutController.GetAbout))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}