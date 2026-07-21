using Eaf.ProjectName.Airplanes.Exporting;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Exporting
{
    public class AirplanesExcelExporter_Tests : ProjectNameTestBase
    {
        private readonly IAirplanesExcelExporter _airplanesExcelExporter;

        public AirplanesExcelExporter_Tests()
        {
            _airplanesExcelExporter = LocalIocManager.Resolve<IAirplanesExcelExporter>();
        }

        [Fact]
        public void Dado_ExporterInicializado_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            _airplanesExcelExporter.ShouldNotBeNull();
        }
    }
}
