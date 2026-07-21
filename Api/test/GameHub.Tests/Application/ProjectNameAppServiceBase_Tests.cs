using GameHub;
using GameHub.Airplanes;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Application
{
    public class ProjectNameAppServiceBase_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_ProjectNameAppServiceBase_Quando_CriarInstanciaEntao_DeveSerValido()
        {
            // ProjectNameAppServiceBase is abstract, so we test the concrete implementation
            var service = LocalIocManager.Resolve<AirplanesAppService>();
            service.ShouldNotBeNull();
            service.ShouldBeAssignableTo<ProjectNameAppServiceBase>();
        }
    }
}
