using Eaf.ProjectName.Airplanes.Dtos;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Airplanes.Dtos
{
    public class GetAirplanesInput_Validation_Tests
    {
        [Fact]
        public void Dado_GetAirplanesInput_Quando_CriarInstancia_Entao_FilterDeveSerNull()
        {
            // Dado/Quando (Given/When)
            var input = new GetAirplanesInput();

            // Então (Then)
            input.Filter.ShouldBeNull();
        }

        [Fact]
        public void Dado_GetAirplanesInput_Quando_DefinirFilter_Entao_DeveAtribuirValor()
        {
            // Dado (Given)
            var input = new GetAirplanesInput();

            // Quando (When)
            input.Filter = "Boeing";

            // Então (Then)
            input.Filter.ShouldBe("Boeing");
        }

        [Fact]
        public void Dado_GetAirplanesInput_Quando_DefinirPaginacao_Entao_DeveAtribuirValores()
        {
            // Dado (Given)
            var input = new GetAirplanesInput
            {
                MaxResultCount = 20,
                SkipCount = 10,
                Sorting = "Model asc"
            };

            // Então (Then)
            input.MaxResultCount.ShouldBe(20);
            input.SkipCount.ShouldBe(10);
            input.Sorting.ShouldBe("Model asc");
        }

        [Fact]
        public void Dado_GetAirplanesInput_Quando_DefinirFilterVazio_Entao_DeveAtribuirStringVazia()
        {
            // Dado (Given)
            var input = new GetAirplanesInput { Filter = "" };

            // Então (Then)
            input.Filter.ShouldBe("");
        }
    }
}
