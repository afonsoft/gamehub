using GameHub.Airplanes;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Airplanes
{
    public class AirplaneManager_Tests : GameHubTestBase
    {
        private readonly IAirplaneManager _airplaneManager;

        public AirplaneManager_Tests()
        {
            _airplaneManager = LocalIocManager.Resolve<IAirplaneManager>();
        }

        [Fact]
        public void Dado_AirplaneManager_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            _airplaneManager.ShouldNotBeNull();
        }
    }
}
