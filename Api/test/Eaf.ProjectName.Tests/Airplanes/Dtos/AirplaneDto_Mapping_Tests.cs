using Eaf.ProjectName.Airplanes.Dtos;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Airplanes.Dtos
{
    public class AirplaneDto_Mapping_Tests
    {
        [Fact]
        public void Dado_AirplaneDto_Quando_CriarInstancia_Entao_DeveTerPropriedadesPadrao()
        {
            // Dado/Quando (Given/When)
            var dto = new AirplaneDto();

            // Então (Then)
            dto.ShouldNotBeNull();
            dto.Id.ShouldBe(0);
            dto.Number.ShouldBeNull();
            dto.Model.ShouldBeNull();
        }

        [Fact]
        public void Dado_AirplaneDto_Quando_DefinirPropriedades_Entao_DeveAtribuirValores()
        {
            // Dado (Given)
            var dto = new AirplaneDto
            {
                Id = 10,
                Number = "MAP-001",
                Model = "Boeing 777-300ER"
            };

            // Então (Then)
            dto.Id.ShouldBe(10);
            dto.Number.ShouldBe("MAP-001");
            dto.Model.ShouldBe("Boeing 777-300ER");
        }
    }
}
