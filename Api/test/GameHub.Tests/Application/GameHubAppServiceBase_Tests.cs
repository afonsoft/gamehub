using GameHub;
using GameHub.Gameplay;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Application
{
    public class GameHubAppServiceBase_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_GameHubAppServiceBase_Quando_CriarInstanciaEntao_DeveSerValido()
        {
            // GameHubAppServiceBase is abstract, so we test a concrete implementation
            var service = LocalIocManager.Resolve<GameplayAppService>();
            service.ShouldNotBeNull();
            service.ShouldBeAssignableTo<GameHubAppServiceBase>();
        }
    }
}
