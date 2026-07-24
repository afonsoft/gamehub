using GameHub.Features;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Features
{
    public class GameHubFeatureProvider_Tests : GameHubTestBase
    {
        private readonly GameHubFeatureProvider _featureProvider;

        public GameHubFeatureProvider_Tests()
        {
            _featureProvider = LocalIocManager.Resolve<GameHubFeatureProvider>();
        }

        [Fact]
        public void Dado_ProviderInicializado_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            // Dado (Given) & Quando (When)
            var provider = _featureProvider;

            // Então (Then)
            provider.ShouldNotBeNull();
        }
    }
}
