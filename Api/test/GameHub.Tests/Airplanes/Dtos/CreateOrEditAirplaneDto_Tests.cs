using GameHub.Airplanes.Dtos;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Airplanes.Dtos
{
    public class CreateOrEditAirplaneDto_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_CreateOrEditAirplaneDto_Quando_Criar_Entao_DeveTerPropriedadesValidas()
        {
            var dto = new CreateOrEditAirplaneDto
            {
                Id = 1,
                Number = "ABC123",
                Model = "Boeing 737"
            };

            dto.ShouldNotBeNull();
            dto.Id.ShouldBe(1);
            dto.Number.ShouldBe("ABC123");
            dto.Model.ShouldBe("Boeing 737");
        }

        [Fact]
        public void Dado_CreateOrEditAirplaneDto_Quando_CriarSemId_Entao_DeveSerParaCriacao()
        {
            var dto = new CreateOrEditAirplaneDto
            {
                Number = "NEW123",
                Model = "Boeing 777"
            };

            dto.Id.ShouldBeNull();
            dto.Number.ShouldBe("NEW123");
            dto.Model.ShouldBe("Boeing 777");
        }

        [Fact]
        public void Dado_CreateOrEditAirplaneDto_Quando_DefinirNumber_Entao_DeveAtribuirValor()
        {
            var dto = new CreateOrEditAirplaneDto { Number = "TST001" };
            dto.Number.ShouldBe("TST001");
        }

        [Fact]
        public void Dado_CreateOrEditAirplaneDto_Quando_DefinirModel_Entao_DeveAtribuirValor()
        {
            var dto = new CreateOrEditAirplaneDto { Model = "Test Model" };
            dto.Model.ShouldBe("Test Model");
        }
    }
}
