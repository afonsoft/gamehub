using GameHub.Configuration;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core.Application.Configuration
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
