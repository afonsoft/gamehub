using GameHub.Authorization;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core.Application.Authorization
{
    public class GameHubPermissions_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_GameHubPermissions_Quando_VerificarConsts_Entao_DeveTerValoresValidos()
        {
            GameHubPermissions.Pages_Airplanes.ShouldNotBeNullOrEmpty();
            GameHubPermissions.Pages_Airplanes_Create.ShouldNotBeNullOrEmpty();
            GameHubPermissions.Pages_Airplanes_Edit.ShouldNotBeNullOrEmpty();
            GameHubPermissions.Pages_Airplanes_Delete.ShouldNotBeNullOrEmpty();
        }
    }
}
