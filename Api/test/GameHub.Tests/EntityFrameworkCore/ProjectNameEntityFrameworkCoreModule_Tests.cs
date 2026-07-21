using GameHub.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace GameHub.Tests.EntityFrameworkCore
{
    public class ProjectNameEntityFrameworkCoreModule_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_ProjectNameEntityFrameworkCoreModule_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            var module = new ProjectNameEntityFrameworkCoreModule();
            module.ShouldNotBeNull();
        }
    }
}
