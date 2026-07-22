using GameHub.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace GameHub.Tests.EntityFrameworkCore
{
    public class GameHubEntityFrameworkCoreModule_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_GameHubEntityFrameworkCoreModule_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            var module = new GameHubEntityFrameworkCoreModule();
            module.ShouldNotBeNull();
        }
    }
}
