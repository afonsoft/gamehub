using Eaf.ProjectName.Configuration;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Core.Application.Configuration
{
    public class ProjectNameSettings_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_ProjectNameSettings_Quando_VerificarConsts_Entao_DeveTerValoresValidos()
        {
            ProjectNameSettings.AirplaneSettings.IsAirplaneManagerEnabled.ShouldNotBeNullOrEmpty();
        }
    }
}
