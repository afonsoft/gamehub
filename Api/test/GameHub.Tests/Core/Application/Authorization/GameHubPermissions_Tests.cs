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
            GameHubPermissions.Pages_Games.ShouldNotBeNullOrEmpty();
            GameHubPermissions.Pages_Games_Create.ShouldNotBeNullOrEmpty();
            GameHubPermissions.Pages_Games_Edit.ShouldNotBeNullOrEmpty();
            GameHubPermissions.Pages_Games_Delete.ShouldNotBeNullOrEmpty();
            GameHubPermissions.Pages_Builds_Upload.ShouldNotBeNullOrEmpty();
            GameHubPermissions.Pages_Gameplay.ShouldNotBeNullOrEmpty();
            GameHubPermissions.Pages_Leaderboard.ShouldNotBeNullOrEmpty();
        }
    }
}
