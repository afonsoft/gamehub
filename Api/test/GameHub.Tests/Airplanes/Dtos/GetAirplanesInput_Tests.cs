using GameHub.Airplanes.Dtos;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Airplanes.Dtos
{
    public class GetAirplanesInput_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_GetAirplanesInput_Quando_Criar_Entao_DeveTerPropriedadesValidas()
        {
            var input = new GetAirplanesInput
            {
                Filter = "Boeing",
                Sorting = "Number asc",
                MaxResultCount = 10,
                SkipCount = 0
            };

            input.ShouldNotBeNull();
            input.Filter.ShouldBe("Boeing");
            input.Sorting.ShouldBe("Number asc");
            input.MaxResultCount.ShouldBe(10);
            input.SkipCount.ShouldBe(0);
        }

        [Fact]
        public void Dado_GetAirplanesInput_Quando_CriarVazio_Entao_DeveTerValoresPadrao()
        {
            var input = new GetAirplanesInput();

            input.Filter.ShouldBeNull();
            input.Sorting.ShouldBeNull();
            input.MaxResultCount.ShouldBeGreaterThanOrEqualTo(0);
            input.SkipCount.ShouldBeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public void Dado_GetAirplanesInput_Quando_DefinirFilter_Entao_DeveAtribuirValor()
        {
            var input = new GetAirplanesInput { Filter = "Airbus" };
            input.Filter.ShouldBe("Airbus");
        }

        [Fact]
        public void Dado_GetAirplanesInput_Quando_DefinirSorting_Entao_DeveAtribuirValor()
        {
            var input = new GetAirplanesInput { Sorting = "Model desc" };
            input.Sorting.ShouldBe("Model desc");
        }

        [Fact]
        public void Dado_GetAirplanesInput_Quando_DefinirMaxResultCount_Entao_DeveAtribuirValor()
        {
            var input = new GetAirplanesInput { MaxResultCount = 50 };
            input.MaxResultCount.ShouldBe(50);
        }

        [Fact]
        public void Dado_GetAirplanesInput_Quando_DefinirSkipCount_Entao_DeveAtribuirValor()
        {
            var input = new GetAirplanesInput { SkipCount = 20 };
            input.SkipCount.ShouldBe(20);
        }
    }
}
