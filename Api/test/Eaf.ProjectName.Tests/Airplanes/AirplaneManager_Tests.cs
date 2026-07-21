using Eaf.ProjectName.Airplanes;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Airplanes
{
    public class AirplaneManager_Tests : ProjectNameTestBase
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
