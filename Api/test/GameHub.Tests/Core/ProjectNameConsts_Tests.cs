using GameHub;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core
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
