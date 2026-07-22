using GameHub.Authorization;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Authorization
{
    public class GameHubAuthorizationProvider_Tests : GameHubTestBase
    {
        private readonly GameHubAuthorizationProvider _authorizationProvider;

        public GameHubAuthorizationProvider_Tests()
        {
            _authorizationProvider = LocalIocManager.Resolve<GameHubAuthorizationProvider>();
        }

        [Fact]
        public void Dado_ProviderInicializado_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            var provider = _authorizationProvider;
            provider.ShouldNotBeNull();
        }
    }
}
