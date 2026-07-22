using GameHub.Gameplay;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Application
{
    public class GameHubAppServiceBase_Localization_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_AppServiceBase_Quando_VerificarTipo_Entao_DeveSerAbstrato()
        {
            var type = typeof(GameHubAppServiceBase);

            type.IsAbstract.ShouldBeTrue();
        }

        [Fact]
        public void Dado_AppServiceBase_Quando_ResolverServico_Entao_DeveEstarDisponivel()
        {
            var service = LocalIocManager.Resolve<IGameplayAppService>();

            service.ShouldNotBeNull();
        }

        [Fact]
        public void Dado_AppServiceBase_Quando_VerificarHeranca_Entao_DeveHerdarDeMiddlewareAppServiceBase()
        {
            typeof(GameHubAppServiceBase)
                .BaseType.Name.ShouldBe("MiddlewareAppServiceBase");
        }
    }
}
