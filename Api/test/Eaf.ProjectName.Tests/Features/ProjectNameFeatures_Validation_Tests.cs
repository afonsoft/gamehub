using Eaf.ProjectName.Features;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Features
{
    public class ProjectNameFeatures_Validation_Tests
    {
        [Fact]
        public void Dado_Features_Quando_VerificarTestCheckFeature_Entao_DeveSerConstanteEsperada()
        {
            ProjectNameFeatures.TestCheckFeature.ShouldBe("App.TestCheckFeature");
        }

        [Fact]
        public void Dado_Features_Quando_VerificarTestCheckFeature_Entao_NaoDeveSerNuloOuVazio()
        {
            ProjectNameFeatures.TestCheckFeature.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public void Dado_Features_Quando_VerificarFormato_Entao_DeveConterPrefixoApp()
        {
            ProjectNameFeatures.TestCheckFeature.ShouldStartWith("App.");
        }
    }
}
