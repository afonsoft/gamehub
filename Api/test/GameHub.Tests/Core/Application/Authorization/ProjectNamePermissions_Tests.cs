using GameHub.Authorization;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core.Application.Authorization
{
    public class ProjectNamePermissions_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_ProjectNamePermissions_Quando_VerificarConsts_Entao_DeveTerValoresValidos()
        {
            ProjectNamePermissions.Pages_Airplanes.ShouldNotBeNullOrEmpty();
            ProjectNamePermissions.Pages_Airplanes_Create.ShouldNotBeNullOrEmpty();
            ProjectNamePermissions.Pages_Airplanes_Edit.ShouldNotBeNullOrEmpty();
            ProjectNamePermissions.Pages_Airplanes_Delete.ShouldNotBeNullOrEmpty();
        }
    }
}
