using GameHub.Features;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Features
{
    public class ProjectNameFeatureProvider_Tests : ProjectNameTestBase
    {
        private readonly ProjectNameFeatureProvider _featureProvider;

        public ProjectNameFeatureProvider_Tests()
        {
            _featureProvider = LocalIocManager.Resolve<ProjectNameFeatureProvider>();
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
