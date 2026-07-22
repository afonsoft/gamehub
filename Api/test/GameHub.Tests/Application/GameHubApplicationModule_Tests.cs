using GameHub;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Application
{
    public class GameHubApplicationModule_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_GameHubApplicationModule_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            var module = new GameHubApplicationModule();
            module.ShouldNotBeNull();
        }
    }
}
