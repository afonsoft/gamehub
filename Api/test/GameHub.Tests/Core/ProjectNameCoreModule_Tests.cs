using GameHub;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core
{
    public class ProjectNameCoreModule_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_ProjectNameCoreModule_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            var module = new ProjectNameCoreModule();
            module.ShouldNotBeNull();
        }
    }
}
