using Eaf.ProjectName.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.EntityFrameworkCore
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
