using GameHub.Configuration;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Configuration
{
    public class ProjectNameSettings_Validation_Tests
    {
        [Fact]
        public void Dado_AirplaneSettings_Quando_VerificarNome_Entao_DeveSerConstanteEsperada()
        {
            ProjectNameSettings.AirplaneSettings.IsAirplaneManagerEnabled
                .ShouldBe("AirplaneSettings.IsAirplaneManagerEnabled");
        }

        [Fact]
        public void Dado_AirplaneSettings_Quando_VerificarNome_Entao_NaoDeveSerNuloOuVazio()
        {
            ProjectNameSettings.AirplaneSettings.IsAirplaneManagerEnabled
                .ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public void Dado_AirplaneSettings_Quando_VerificarFormato_Entao_DeveConterPrefixo()
        {
            ProjectNameSettings.AirplaneSettings.IsAirplaneManagerEnabled
                .ShouldStartWith("AirplaneSettings.");
        }
    }
}
