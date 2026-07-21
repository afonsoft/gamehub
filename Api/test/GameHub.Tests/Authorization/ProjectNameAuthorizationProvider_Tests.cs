using GameHub.Authorization;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Authorization
{
    public class ProjectNameAuthorizationProvider_Tests : ProjectNameTestBase
    {
        private readonly ProjectNameAuthorizationProvider _authorizationProvider;

        public ProjectNameAuthorizationProvider_Tests()
        {
            _authorizationProvider = LocalIocManager.Resolve<ProjectNameAuthorizationProvider>();
        }

        [Fact]
        public void Dado_ProviderInicializado_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            var provider = _authorizationProvider;
            provider.ShouldNotBeNull();
        }
    }
}
