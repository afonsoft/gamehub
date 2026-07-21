using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.General
{
    /// <summary>
    /// Testes para extensões de string seguindo o padrão BDD (Given/When/Then) em português
    /// </summary>
    public class StringExtensions_Tests
    {
        [Fact]
        public void Dado_StringVazia_Quando_ChamarIsNullOrEmpty_Entao_DeveRetornarVerdadeiro()
        {
            // Dado (Given)
            var texto = string.Empty;

            // Quando (When)
            var resultado = string.IsNullOrEmpty(texto);

            // Então (Then)
            resultado.ShouldBeTrue();
        }

        [Fact]
        public void Dado_StringNula_Quando_ChamarIsNullOrEmpty_Entao_DeveRetornarVerdadeiro()
        {
            // Dado (Given)
            string texto = null;

            // Quando (When)
            var resultado = string.IsNullOrEmpty(texto);

            // Então (Then)
            resultado.ShouldBeTrue();
        }

        [Fact]
        public void Dado_StringComConteudo_Quando_ChamarIsNullOrEmpty_Entao_DeveRetornarFalso()
        {
            // Dado (Given)
            var texto = "EAF Test";

            // Quando (When)
            var resultado = string.IsNullOrEmpty(texto);

            // Então (Then)
            resultado.ShouldBeFalse();
        }

        [Fact]
        public void Dado_StringComEspacos_Quando_ChamarTrim_Entao_DeveRemoverEspacos()
        {
            // Dado (Given)
            var texto = "  EAF  ";

            // Quando (When)
            var resultado = texto.Trim();

            // Então (Then)
            resultado.ShouldBe("EAF");
        }

        [Fact]
        public void Dado_StringMaiuscula_Quando_ChamarToLower_Entao_DeveConverterParaMinuscula()
        {
            // Dado (Given)
            var texto = "EAF";

            // Quando (When)
            var resultado = texto.ToLower();

            // Então (Then)
            resultado.ShouldBe("eaf");
        }

        [Fact]
        public void Dado_StringMinuscula_Quando_ChamarToUpper_Entao_DeveConverterParaMaiuscula()
        {
            // Dado (Given)
            var texto = "eaf";

            // Quando (When)
            var resultado = texto.ToUpper();

            // Então (Then)
            resultado.ShouldBe("EAF");
        }
    }
}
