using Eaf.ProjectName.Airplanes;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Core
{
    public class ProjectNameDomainServiceBase_Localization_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_DomainService_Quando_Resolver_Entao_DeveEstarConfigurado()
        {
            var manager = LocalIocManager.Resolve<IAirplaneManager>();

            manager.ShouldNotBeNull();
        }

        [Fact]
        public void Dado_DomainServiceBase_Quando_VerificarTipo_Entao_DeveSerAbstrato()
        {
            var type = typeof(ProjectNameDomainServiceBase);

            type.IsAbstract.ShouldBeTrue();
        }

        [Fact]
        public void Dado_DomainServiceBase_Quando_VerificarHeranca_Entao_DeveHerdarDeDomainService()
        {
            typeof(ProjectNameDomainServiceBase)
                .BaseType.Name.ShouldBe("DomainService");
        }
    }
}
