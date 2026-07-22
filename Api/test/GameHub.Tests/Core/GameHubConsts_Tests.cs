using GameHub;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core
{
    public class GameHubConsts_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_GameHubConsts_Quando_VerificarConsts_Entao_DeveTerValoresValidos()
        {
            GameHubConsts.LocalizationSourceName.ShouldNotBeNullOrEmpty();
            GameHubConsts.ConnectionStringName.ShouldNotBeNullOrEmpty();
        }
    }
}
