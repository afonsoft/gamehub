using GameHub;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core
{
    public class GameHubDomainServiceBase_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_GameHubDomainServiceBase_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            // GameHubDomainServiceBase is abstract, so we test that it exists
            var type = typeof(GameHubDomainServiceBase);
            type.ShouldNotBeNull();
            type.IsAbstract.ShouldBeTrue();
        }
    }
}
