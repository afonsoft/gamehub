using GameHub.Moderation;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class ProfanityFilter_Tests
    {
        private readonly ProfanityFilter _filter = new ProfanityFilter();

        [Fact]
        public void Dado_TextoSemPalavrao_Quando_Verificar_Entao_DeveRetornarFalso()
        {
            _filter.ContainsProfanity("Jogo muito divertido e bem feito").ShouldBeFalse();
        }

        [Fact]
        public void Dado_TextoComPalavrao_Quando_Verificar_Entao_DeveRetornarVerdadeiro()
        {
            _filter.ContainsProfanity("Esse jogo é uma merda").ShouldBeTrue();
        }

        [Fact]
        public void Dado_TextoComLeet_Quando_Verificar_Entao_DeveDetectar()
        {
            _filter.ContainsProfanity("Esse jogo é um sh1t").ShouldBeTrue();
        }

        [Theory]
        [InlineData("Texto merda censurado", "Texto ***** censurado")]
        public void Dado_TextoComPalavrao_Quando_Censurar_Entao_DeveOcultar(string input, string expected)
        {
            var result = _filter.Censor(input);
            result.ShouldBe(expected);
        }
    }
}
