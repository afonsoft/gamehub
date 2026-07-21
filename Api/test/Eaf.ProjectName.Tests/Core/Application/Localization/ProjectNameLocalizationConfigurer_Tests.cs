using Eaf.ProjectName.Localization;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Core.Application.Localization
{
    public class ProjectNameLocalizationConfigurer_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_ProjectNameLocalizationConfigurer_Quando_VerificarTipo_Entao_DeveSerClasseEstatica()
        {
            var type = typeof(ProjectNameLocalizationConfigurer);
            type.ShouldNotBeNull();
            type.IsAbstract.ShouldBeTrue();
            type.IsSealed.ShouldBeTrue();
        }
    }
}
