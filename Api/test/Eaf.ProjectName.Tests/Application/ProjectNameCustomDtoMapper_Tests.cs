using Abp.ObjectMapping;
using Eaf.ProjectName.Airplanes;
using Eaf.ProjectName.Airplanes.Dtos;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Application
{
    public class ProjectNameCustomDtoMapper_Tests : ProjectNameTestBase
    {
        private readonly IObjectMapper _objectMapper;

        public ProjectNameCustomDtoMapper_Tests()
        {
            _objectMapper = LocalIocManager.Resolve<IObjectMapper>();
        }

        [Fact]
        public void Dado_MapperConfigurado_Quando_MapearAirplaneParaDto_Entao_DeveMapearCorretamente()
        {
            var airplane = new Airplane
            {
                Number = "MAP-001",
                Model = "Boeing 737",
                TenantId = 1
            };

            var dto = _objectMapper.Map<AirplaneDto>(airplane);

            dto.ShouldNotBeNull();
            dto.Number.ShouldBe("MAP-001");
            dto.Model.ShouldBe("Boeing 737");
        }

        [Fact]
        public void Dado_MapperConfigurado_Quando_MapearDtoParaAirplane_Entao_DeveMapearCorretamente()
        {
            var dto = new CreateOrEditAirplaneDto
            {
                Number = "MAP-002",
                Model = "Airbus A320"
            };

            var airplane = _objectMapper.Map<Airplane>(dto);

            airplane.ShouldNotBeNull();
            airplane.Number.ShouldBe("MAP-002");
            airplane.Model.ShouldBe("Airbus A320");
        }

        [Fact]
        public void Dado_MapperConfigurado_Quando_MapearComPropriedadesNulas_Entao_DeveMapear()
        {
            var airplane = new Airplane
            {
                Number = "MAP-003",
                Model = "Cessna 172",
                TenantId = null
            };

            var dto = _objectMapper.Map<AirplaneDto>(airplane);

            dto.ShouldNotBeNull();
            dto.Number.ShouldBe("MAP-003");
            dto.Model.ShouldBe("Cessna 172");
        }
    }
}
