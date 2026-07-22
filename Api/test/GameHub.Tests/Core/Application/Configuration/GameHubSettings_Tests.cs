using GameHub.Configuration;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core.Application.Configuration
{
    public class GameHubSettings_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_GameHubSettings_Quando_VerificarConsts_Entao_DeveTerValoresValidos()
        {
            GameHubSettings.AirplaneSettings.IsAirplaneManagerEnabled.ShouldNotBeNullOrEmpty();
        }
    }
}
