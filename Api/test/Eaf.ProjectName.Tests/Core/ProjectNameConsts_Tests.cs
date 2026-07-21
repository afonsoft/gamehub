using Eaf.ProjectName;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Core
{
    public class ProjectNameConsts_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_ProjectNameConsts_Quando_VerificarConsts_Entao_DeveTerValoresValidos()
        {
            ProjectNameConsts.LocalizationSourceName.ShouldNotBeNullOrEmpty();
            ProjectNameConsts.ConnectionStringName.ShouldNotBeNullOrEmpty();
        }
    }
}
