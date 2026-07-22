using GameHub;
using GameHub.Airplanes;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Application
{
    public class GameHubAppServiceBase_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_GameHubAppServiceBase_Quando_CriarInstanciaEntao_DeveSerValido()
        {
            // GameHubAppServiceBase is abstract, so we test the concrete implementation
            var service = LocalIocManager.Resolve<AirplanesAppService>();
            service.ShouldNotBeNull();
            service.ShouldBeAssignableTo<GameHubAppServiceBase>();
        }
    }
}
