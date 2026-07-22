using GameHub.Features;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core.Application.Features
{
    public class GameHubFeatures_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_GameHubFeatures_Quando_VerificarConsts_Entao_DeveTerValoresValidos()
        {
            GameHubFeatures.TestCheckFeature.ShouldNotBeNullOrEmpty();
        }
    }
}
