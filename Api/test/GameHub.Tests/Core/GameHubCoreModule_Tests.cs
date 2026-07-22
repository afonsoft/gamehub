using GameHub;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core
{
    public class GameHubCoreModule_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_GameHubCoreModule_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            var module = new GameHubCoreModule();
            module.ShouldNotBeNull();
        }
    }
}
