using GameHub;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core
{
    public class ProjectNameDomainServiceBase_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_ProjectNameDomainServiceBase_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            // ProjectNameDomainServiceBase is abstract, so we test that it exists
            var type = typeof(ProjectNameDomainServiceBase);
            type.ShouldNotBeNull();
            type.IsAbstract.ShouldBeTrue();
        }
    }
}
