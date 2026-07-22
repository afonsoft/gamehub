using GameHub.Configuration;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Configuration
{
    public class GameHubSettings_Validation_Tests
    {
        [Fact]
        public void Dado_AirplaneSettings_Quando_VerificarNome_Entao_DeveSerConstanteEsperada()
        {
            GameHubSettings.AirplaneSettings.IsAirplaneManagerEnabled
                .ShouldBe("AirplaneSettings.IsAirplaneManagerEnabled");
        }

        [Fact]
        public void Dado_AirplaneSettings_Quando_VerificarNome_Entao_NaoDeveSerNuloOuVazio()
        {
            GameHubSettings.AirplaneSettings.IsAirplaneManagerEnabled
                .ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public void Dado_AirplaneSettings_Quando_VerificarFormato_Entao_DeveConterPrefixo()
        {
            GameHubSettings.AirplaneSettings.IsAirplaneManagerEnabled
                .ShouldStartWith("AirplaneSettings.");
        }
    }
}
