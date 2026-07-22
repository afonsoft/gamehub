using GameHub.Notifications;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core.Application.Notifications
{
    public class GameHubNotificationNames_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_GameHubNotificationNames_Quando_VerificarConsts_Entao_DeveTerValoresValidos()
        {
            GameHubNotificationNames.SimpleMessage.ShouldNotBeNullOrEmpty();
        }
    }
}
