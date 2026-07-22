using GameHub.Localization;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core.Application.Localization
{
    public class GameHubLocalizationConfigurer_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_GameHubLocalizationConfigurer_Quando_VerificarTipo_Entao_DeveSerClasseEstatica()
        {
            var type = typeof(GameHubLocalizationConfigurer);
            type.ShouldNotBeNull();
            type.IsAbstract.ShouldBeTrue();
            type.IsSealed.ShouldBeTrue();
        }
    }
}
