using GameHub.Features;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Features
{
    public class GameHubFeatures_Validation_Tests
    {
        [Fact]
        public void Dado_Features_Quando_VerificarTestCheckFeature_Entao_DeveSerConstanteEsperada()
        {
            GameHubFeatures.TestCheckFeature.ShouldBe("App.TestCheckFeature");
        }

        [Fact]
        public void Dado_Features_Quando_VerificarTestCheckFeature_Entao_NaoDeveSerNuloOuVazio()
        {
            GameHubFeatures.TestCheckFeature.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public void Dado_Features_Quando_VerificarFormato_Entao_DeveConterPrefixoApp()
        {
            GameHubFeatures.TestCheckFeature.ShouldStartWith("App.");
        }
    }
}
