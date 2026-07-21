using GameHub.Airplanes.Dtos;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Airplanes.Dtos
{
    public class AirplaneDto_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_AirplaneDto_Quando_Criar_Entao_DeveTerPropriedadesValidas()
        {
            var dto = new AirplaneDto
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
        public void Dado_AirplaneDto_Quando_DefinirId_Entao_DeveAtribuirValor()
        {
            var dto = new AirplaneDto { Id = 100 };
            dto.Id.ShouldBe(100);
        }

        [Fact]
        public void Dado_AirplaneDto_Quando_DefinirNumber_Entao_DeveAtribuirValor()
        {
            var dto = new AirplaneDto { Number = "XYZ789" };
            dto.Number.ShouldBe("XYZ789");
        }

        [Fact]
        public void Dado_AirplaneDto_Quando_DefinirModel_Entao_DeveAtribuirValor()
        {
            var dto = new AirplaneDto { Model = "Airbus A320" };
            dto.Model.ShouldBe("Airbus A320");
        }
    }
}
