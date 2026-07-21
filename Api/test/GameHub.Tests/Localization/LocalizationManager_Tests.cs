using Abp.Localization;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Localization
{
    /// <summary>
    /// Testes para gerenciamento de localização seguindo o padrão BDD (Given/When/Then) em português
    /// </summary>
    public class LocalizationManager_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_SistemaInicializado_Quando_ObterFonteDeLocalizacao_Entao_DeveRetornarFonteValida()
        {
            // Dado (Given)
            var localizationManager = LocalIocManager.Resolve<ILocalizationManager>();

            // Quando (When)
            var source = localizationManager.GetSource("ProjectName");

            // Então (Then)
            source.ShouldNotBeNull();
            source.Name.ShouldBe("ProjectName");
        }

        [Fact]
        public void Dado_SistemaInicializado_Quando_ObterTextoLocalizado_Entao_DeveRetornarTextoValido()
        {
            // Dado (Given)
            var localizationManager = LocalIocManager.Resolve<ILocalizationManager>();

            // Quando (When)
            var texto = localizationManager.GetString("ProjectName", "Welcome");

            // Então (Then)
            texto.ShouldNotBeNull();
            texto.ShouldNotBeEmpty();
        }

        [Fact]
        public void Dado_SistemaInicializado_Quando_ObterTextoComChaveInexistente_Entao_DeveRetornarChaveEntreColchetes()
        {
            // Dado (Given)
            var localizationManager = LocalIocManager.Resolve<ILocalizationManager>();

            // Quando (When)
            var texto = localizationManager.GetString("ProjectName", "ChaveInexistente");

            // Então (Then)
            texto.ShouldBe("[Chave inexistente]");
        }
    }
}
