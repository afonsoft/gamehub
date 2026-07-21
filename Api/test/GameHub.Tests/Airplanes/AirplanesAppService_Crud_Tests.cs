using Abp.Application.Services.Dto;
using Abp.Domain.Uow;
using GameHub.Airplanes;
using GameHub.Airplanes.Dtos;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace GameHub.Tests.Airplanes
{
    public class AirplanesAppService_Crud_Tests : ProjectNameTestBase
    {
        private readonly AirplanesAppService _airplanesAppService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public AirplanesAppService_Crud_Tests()
        {
            _airplanesAppService = LocalIocManager.Resolve<AirplanesAppService>();
            _airplanesAppService.AbpSession = AbpSession;
            _unitOfWorkManager = LocalIocManager.Resolve<IUnitOfWorkManager>();
        }

        [Fact]
        public async Task Dado_AirplanesAppService_Quando_CriarAeronave_Entao_DeveSerCriada()
        {
            var input = new CreateOrEditAirplaneDto
            {
                Number = "SVC-001",
                Model = "Boeing 737-800"
            };

            using (var uow = _unitOfWorkManager.Begin())
            {
                await _airplanesAppService.CreateOrEdit(input);
                await uow.CompleteAsync();
            }

            using (var uow = _unitOfWorkManager.Begin())
            {
                var result = await _airplanesAppService.GetAll(new GetAirplanesInput
                {
                    Filter = "SVC-001",
                    MaxResultCount = 10,
                    SkipCount = 0
                });
                result.TotalCount.ShouldBeGreaterThan(0);
                await uow.CompleteAsync();
            }
        }

        [Fact]
        public async Task Dado_AirplanesAppService_Quando_ListarTodas_Entao_DeveRetornarResultadoPaginado()
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                await _airplanesAppService.CreateOrEdit(new CreateOrEditAirplaneDto
                {
                    Number = "LIST-001",
                    Model = "Airbus A320"
                });
                await uow.CompleteAsync();
            }

            PagedResultDto<AirplaneDto> result;
            using (var uow = _unitOfWorkManager.Begin())
            {
                result = await _airplanesAppService.GetAll(new GetAirplanesInput
                {
                    MaxResultCount = 10,
                    SkipCount = 0
                });
                await uow.CompleteAsync();
            }

            result.ShouldNotBeNull();
            result.TotalCount.ShouldBeGreaterThan(0);
            result.Items.ShouldNotBeNull();
            result.Items.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task Dado_AirplanesAppService_Quando_FiltrarPorNumero_Entao_DeveRetornarAeronavesFiltradas()
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                await _airplanesAppService.CreateOrEdit(new CreateOrEditAirplaneDto
                {
                    Number = "FILTER-AAA",
                    Model = "Boeing 777"
                });
                await _airplanesAppService.CreateOrEdit(new CreateOrEditAirplaneDto
                {
                    Number = "FILTER-BBB",
                    Model = "Airbus A380"
                });
                await uow.CompleteAsync();
            }

            PagedResultDto<AirplaneDto> result;
            using (var uow = _unitOfWorkManager.Begin())
            {
                result = await _airplanesAppService.GetAll(new GetAirplanesInput
                {
                    Filter = "FILTER-AAA",
                    MaxResultCount = 10,
                    SkipCount = 0
                });
                await uow.CompleteAsync();
            }

            result.TotalCount.ShouldBe(1);
            result.Items[0].Number.ShouldBe("FILTER-AAA");
        }

        [Fact]
        public async Task Dado_AirplanesAppService_Quando_FiltrarPorModelo_Entao_DeveRetornarAeronavesFiltradas()
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                await _airplanesAppService.CreateOrEdit(new CreateOrEditAirplaneDto
                {
                    Number = "MOD-001",
                    Model = "Embraer E195-UniqueModel"
                });
                await uow.CompleteAsync();
            }

            PagedResultDto<AirplaneDto> result;
            using (var uow = _unitOfWorkManager.Begin())
            {
                result = await _airplanesAppService.GetAll(new GetAirplanesInput
                {
                    Filter = "UniqueModel",
                    MaxResultCount = 10,
                    SkipCount = 0
                });
                await uow.CompleteAsync();
            }

            result.TotalCount.ShouldBe(1);
            result.Items[0].Model.ShouldContain("UniqueModel");
        }

        [Fact]
        public async Task Dado_AirplanesAppService_Quando_ObterParaEditar_Entao_DeveRetornarDto()
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                await _airplanesAppService.CreateOrEdit(new CreateOrEditAirplaneDto
                {
                    Number = "EDIT-001",
                    Model = "Boeing 787 Dreamliner"
                });
                await uow.CompleteAsync();
            }

            int airplaneId;
            using (var uow = _unitOfWorkManager.Begin())
            {
                var all = await _airplanesAppService.GetAll(new GetAirplanesInput
                {
                    Filter = "EDIT-001",
                    MaxResultCount = 1,
                    SkipCount = 0
                });
                airplaneId = all.Items[0].Id;
                await uow.CompleteAsync();
            }

            CreateOrEditAirplaneDto result;
            using (var uow = _unitOfWorkManager.Begin())
            {
                result = await _airplanesAppService.GetAirplaneForEdit(new EntityDto(airplaneId));
                await uow.CompleteAsync();
            }

            result.ShouldNotBeNull();
            result.Number.ShouldBe("EDIT-001");
            result.Model.ShouldBe("Boeing 787 Dreamliner");
            result.Id.ShouldBe(airplaneId);
        }

        [Fact]
        public async Task Dado_AirplanesAppService_Quando_AtualizarAeronave_Entao_DeveRefletirAlteracoes()
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                await _airplanesAppService.CreateOrEdit(new CreateOrEditAirplaneDto
                {
                    Number = "UPDSVC-001",
                    Model = "Boeing 737"
                });
                await uow.CompleteAsync();
            }

            int airplaneId;
            using (var uow = _unitOfWorkManager.Begin())
            {
                var all = await _airplanesAppService.GetAll(new GetAirplanesInput
                {
                    Filter = "UPDSVC-001",
                    MaxResultCount = 1,
                    SkipCount = 0
                });
                airplaneId = all.Items[0].Id;
                await uow.CompleteAsync();
            }

            using (var uow = _unitOfWorkManager.Begin())
            {
                await _airplanesAppService.CreateOrEdit(new CreateOrEditAirplaneDto
                {
                    Id = airplaneId,
                    Number = "UPDSVC-001",
                    Model = "Boeing 737 MAX"
                });
                await uow.CompleteAsync();
            }

            using (var uow = _unitOfWorkManager.Begin())
            {
                var result = await _airplanesAppService.GetAirplaneForEdit(new EntityDto(airplaneId));
                result.Model.ShouldBe("Boeing 737 MAX");
                await uow.CompleteAsync();
            }
        }

        [Fact]
        public async Task Dado_AirplanesAppService_Quando_DeletarAeronave_Entao_NaoDeveSerListada()
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                await _airplanesAppService.CreateOrEdit(new CreateOrEditAirplaneDto
                {
                    Number = "DELSVC-001",
                    Model = "Airbus A350"
                });
                await uow.CompleteAsync();
            }

            int airplaneId;
            using (var uow = _unitOfWorkManager.Begin())
            {
                var all = await _airplanesAppService.GetAll(new GetAirplanesInput
                {
                    Filter = "DELSVC-001",
                    MaxResultCount = 1,
                    SkipCount = 0
                });
                airplaneId = all.Items[0].Id;
                await uow.CompleteAsync();
            }

            using (var uow = _unitOfWorkManager.Begin())
            {
                await _airplanesAppService.Delete(new EntityDto(airplaneId));
                await uow.CompleteAsync();
            }

            using (var uow = _unitOfWorkManager.Begin())
            {
                var result = await _airplanesAppService.GetAll(new GetAirplanesInput
                {
                    Filter = "DELSVC-001",
                    MaxResultCount = 10,
                    SkipCount = 0
                });
                result.TotalCount.ShouldBe(0);
                await uow.CompleteAsync();
            }
        }

        [Fact]
        public async Task Dado_AirplanesAppService_Quando_ExportarParaExcel_Entao_DeveRetornarArquivo()
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                await _airplanesAppService.CreateOrEdit(new CreateOrEditAirplaneDto
                {
                    Number = "EXCEL-001",
                    Model = "Boeing 747-400"
                });
                await uow.CompleteAsync();
            }

            Eaf.Middleware.Dto.FileDto result;
            using (var uow = _unitOfWorkManager.Begin())
            {
                result = await _airplanesAppService.GetAirplanesToExcel();
                await uow.CompleteAsync();
            }

            result.ShouldNotBeNull();
            result.FileName.ShouldNotBeNullOrEmpty();
            result.FileName.ShouldContain("Airplanes");
            result.FileType.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async Task Dado_AirplanesAppService_Quando_ListarSemFiltro_Entao_DeveRetornarTodas()
        {
            int initialCount;
            using (var uow = _unitOfWorkManager.Begin())
            {
                initialCount = (await _airplanesAppService.GetAll(new GetAirplanesInput
                {
                    MaxResultCount = 1000,
                    SkipCount = 0
                })).TotalCount;
                await uow.CompleteAsync();
            }

            using (var uow = _unitOfWorkManager.Begin())
            {
                await _airplanesAppService.CreateOrEdit(new CreateOrEditAirplaneDto
                {
                    Number = "NOFILT-001",
                    Model = "Cessna 172"
                });
                await uow.CompleteAsync();
            }

            PagedResultDto<AirplaneDto> result;
            using (var uow = _unitOfWorkManager.Begin())
            {
                result = await _airplanesAppService.GetAll(new GetAirplanesInput
                {
                    MaxResultCount = 1000,
                    SkipCount = 0
                });
                await uow.CompleteAsync();
            }

            result.TotalCount.ShouldBe(initialCount + 1);
        }

        [Fact]
        public async Task Dado_AirplanesAppService_Quando_Paginar_Entao_DeveRespeitarPaginacao()
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                for (int i = 1; i <= 5; i++)
                {
                    await _airplanesAppService.CreateOrEdit(new CreateOrEditAirplaneDto
                    {
                        Number = $"PAGE-{i:D3}",
                        Model = $"Modelo Paginacao {i}"
                    });
                }
                await uow.CompleteAsync();
            }

            PagedResultDto<AirplaneDto> page1, page2;
            using (var uow = _unitOfWorkManager.Begin())
            {
                page1 = await _airplanesAppService.GetAll(new GetAirplanesInput
                {
                    Filter = "PAGE-",
                    MaxResultCount = 2,
                    SkipCount = 0
                });
                page2 = await _airplanesAppService.GetAll(new GetAirplanesInput
                {
                    Filter = "PAGE-",
                    MaxResultCount = 2,
                    SkipCount = 2
                });
                await uow.CompleteAsync();
            }

            page1.Items.Count.ShouldBe(2);
            page2.Items.Count.ShouldBe(2);
            page1.TotalCount.ShouldBe(5);
        }
    }
}
