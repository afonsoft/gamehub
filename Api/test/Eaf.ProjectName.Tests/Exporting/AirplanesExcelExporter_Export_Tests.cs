using Eaf.ProjectName.Airplanes.Dtos;
using Eaf.ProjectName.Airplanes.Exporting;
using Shouldly;
using System.Collections.Generic;
using Xunit;

namespace Eaf.ProjectName.Tests.Exporting
{
    public class AirplanesExcelExporter_Export_Tests : ProjectNameTestBase
    {
        private readonly IAirplanesExcelExporter _airplanesExcelExporter;

        public AirplanesExcelExporter_Export_Tests()
        {
            _airplanesExcelExporter = LocalIocManager.Resolve<IAirplanesExcelExporter>();
        }

        [Fact]
        public void Dado_ListaDeAeronaves_Quando_ExportarParaExcel_Entao_DeveRetornarArquivoValido()
        {
            var airplanes = new List<AirplaneDto>
            {
                new AirplaneDto { Id = 1, Number = "EXP-001", Model = "Boeing 737" },
                new AirplaneDto { Id = 2, Number = "EXP-002", Model = "Airbus A320" }
            };

            var result = _airplanesExcelExporter.ExportToFile(airplanes);

            result.ShouldNotBeNull();
            result.FileName.ShouldNotBeNullOrEmpty();
            result.FileName.ShouldContain("Airplanes");
            result.FileType.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public void Dado_ListaVazia_Quando_ExportarParaExcel_Entao_DeveRetornarArquivoValido()
        {
            var airplanes = new List<AirplaneDto>();

            var result = _airplanesExcelExporter.ExportToFile(airplanes);

            result.ShouldNotBeNull();
            result.FileName.ShouldNotBeNullOrEmpty();
            result.FileName.ShouldContain("Airplanes");
        }

        [Fact]
        public void Dado_ListaComUmaAeronave_Quando_ExportarParaExcel_Entao_DeveRetornarArquivoValido()
        {
            var airplanes = new List<AirplaneDto>
            {
                new AirplaneDto { Id = 1, Number = "SINGLE-001", Model = "Embraer E195" }
            };

            var result = _airplanesExcelExporter.ExportToFile(airplanes);

            result.ShouldNotBeNull();
            result.FileName.ShouldContain("Airplanes");
            result.FileType.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public void Dado_ListaGrande_Quando_ExportarParaExcel_Entao_DeveRetornarArquivoValido()
        {
            var airplanes = new List<AirplaneDto>();
            for (int i = 1; i <= 50; i++)
            {
                airplanes.Add(new AirplaneDto
                {
                    Id = i,
                    Number = $"BULK-{i:D3}",
                    Model = $"Modelo {i}"
                });
            }

            var result = _airplanesExcelExporter.ExportToFile(airplanes);

            result.ShouldNotBeNull();
            result.FileName.ShouldContain("Airplanes");
        }
    }
}
