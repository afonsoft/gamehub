using Eaf.ProjectName.Features;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Core.Application.Features
{
    public class ProjectNameFeatures_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_ProjectNameFeatures_Quando_VerificarConsts_Entao_DeveTerValoresValidos()
        {
            ProjectNameFeatures.TestCheckFeature.ShouldNotBeNullOrEmpty();
        }
    }
}
