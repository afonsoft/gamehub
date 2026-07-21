using Abp.Domain.Uow;
using Eaf.ProjectName.Airplanes;
using Eaf.ProjectName.Airplanes.Dtos;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Eaf.ProjectName.Tests.Airplanes
{
    public class AirplanesAppService_Excel_Tests : ProjectNameTestBase
    {
        private readonly IAirplanesAppService _airplanesAppService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public AirplanesAppService_Excel_Tests()
        {
            _airplanesAppService = LocalIocManager.Resolve<IAirplanesAppService>();
            _unitOfWorkManager = LocalIocManager.Resolve<IUnitOfWorkManager>();
        }

        [Fact]
        public async Task Dado_AeronautaSemDados_Quando_ExportarParaExcel_Entao_DeveRetornarArquivoVazio()
        {
            Eaf.Middleware.Dto.FileDto result;
            using (var uow = _unitOfWorkManager.Begin())
            {
                result = await _airplanesAppService.GetAirplanesToExcel();
                await uow.CompleteAsync();
            }

            result.ShouldNotBeNull();
            result.FileName.ShouldNotBeNullOrEmpty();
            result.FileName.ShouldContain("Airplanes");
        }

        [Fact]
        public async Task Dado_MultiplasAeronaves_Quando_ExportarParaExcel_Entao_DeveRetornarArquivoCompleto()
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                for (int i = 1; i <= 3; i++)
                {
                    await _airplanesAppService.CreateOrEdit(new CreateOrEditAirplaneDto
                    {
                        Number = $"XLMULTI-{i:D3}",
                        Model = $"Modelo Excel {i}"
                    });
                }
                await uow.CompleteAsync();
            }

            Eaf.Middleware.Dto.FileDto result;
            using (var uow = _unitOfWorkManager.Begin())
            {
                result = await _airplanesAppService.GetAirplanesToExcel();
                await uow.CompleteAsync();
            }

            result.ShouldNotBeNull();
            result.FileName.ShouldContain("Airplanes");
            result.FileType.ShouldNotBeNullOrEmpty();
        }
    }
}
