using Eaf.ProjectName.Airplanes;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Application
{
    public class ProjectNameAppServiceBase_Localization_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_AppServiceBase_Quando_VerificarTipo_Entao_DeveSerAbstrato()
        {
            var type = typeof(ProjectNameAppServiceBase);

            type.IsAbstract.ShouldBeTrue();
        }

        [Fact]
        public void Dado_AppServiceBase_Quando_ResolverServico_Entao_DeveEstarDisponivel()
        {
            var service = LocalIocManager.Resolve<IAirplanesAppService>();

            service.ShouldNotBeNull();
        }

        [Fact]
        public void Dado_AppServiceBase_Quando_VerificarHeranca_Entao_DeveHerdarDeMiddlewareAppServiceBase()
        {
            typeof(ProjectNameAppServiceBase)
                .BaseType.Name.ShouldBe("MiddlewareAppServiceBase");
        }
    }
}
