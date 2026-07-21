using Eaf.ProjectName;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Application
{
    public class ProjectNameApplicationModule_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_ProjectNameApplicationModule_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            var module = new ProjectNameApplicationModule();
            module.ShouldNotBeNull();
        }
    }
}
