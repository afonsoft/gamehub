using GameHub.Airplanes;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Entities
{
    public class Airplane_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_Airplane_Quando_Criar_Entao_DeveTerPropriedadesValidas()
        {
            // Dado (Given)
            var airplane = new Airplane
            {
                Number = "TEST123",
                Model = "Boeing 737"
            };

            // Então (Then)
            airplane.ShouldNotBeNull();
            airplane.Number.ShouldBe("TEST123");
            airplane.Model.ShouldBe("Boeing 737");
        }

        [Fact]
        public void Dado_Airplane_Quando_DefinirTenantId_Entao_DeveAtribuirValor()
        {
            // Dado (Given)
            var airplane = new Airplane
            {
                Number = "TEST456",
                Model = "Airbus A320",
                TenantId = 1
            };

            // Então (Then)
            airplane.TenantId.ShouldBe(1);
        }

        [Fact]
        public void Dado_Airplane_Quando_DefinirId_Entao_DeveAtribuirValor()
        {
            // Dado (Given)
            var airplane = new Airplane
            {
                Id = 100,
                Number = "TEST789",
                Model = "Boeing 777"
            };

            // Então (Then)
            airplane.Id.ShouldBe(100);
        }
    }
}
