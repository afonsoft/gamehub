using Abp.Domain.Uow;
using GameHub.Airplanes;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GameHub.Tests.Airplanes
{
    public class AirplaneManager_Crud_Tests : GameHubTestBase
    {
        private readonly IAirplaneManager _airplaneManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public AirplaneManager_Crud_Tests()
        {
            _airplaneManager = LocalIocManager.Resolve<IAirplaneManager>();
            _unitOfWorkManager = LocalIocManager.Resolve<IUnitOfWorkManager>();
        }

        [Fact]
        public async Task Dado_AirplaneManager_Quando_CriarAeronave_Entao_DeveRetornarEntidadeSalva()
        {
            Airplane result = null;
            using (var uow = _unitOfWorkManager.Begin())
            {
                var airplane = new Airplane
                {
                    Number = "CRUD-001",
                    Model = "Boeing 737-800",
                    TenantId = AbpSession.TenantId
                };
                result = await _airplaneManager.CreateAsync(airplane);
                await uow.CompleteAsync();
            }

            result.ShouldNotBeNull();
            result.Id.ShouldBeGreaterThan(0);
            result.Number.ShouldBe("CRUD-001");
            result.Model.ShouldBe("Boeing 737-800");
        }

        [Fact]
        public async Task Dado_AirplaneManager_Quando_BuscarPorId_Entao_DeveRetornarAeronaveCorreta()
        {
            int createdId;
            using (var uow = _unitOfWorkManager.Begin())
            {
                var airplane = new Airplane
                {
                    Number = "FIND-001",
                    Model = "Airbus A320",
                    TenantId = AbpSession.TenantId
                };
                var created = await _airplaneManager.CreateAsync(airplane);
                createdId = created.Id;
                await uow.CompleteAsync();
            }

            Airplane result;
            using (var uow = _unitOfWorkManager.Begin())
            {
                result = await _airplaneManager.GetByIdAsync(createdId);
                await uow.CompleteAsync();
            }

            result.ShouldNotBeNull();
            result.Id.ShouldBe(createdId);
            result.Number.ShouldBe("FIND-001");
            result.Model.ShouldBe("Airbus A320");
        }

        [Fact]
        public async Task Dado_AirplaneManager_Quando_BuscarPorIdInexistente_Entao_DeveRetornarNull()
        {
            Airplane result;
            using (var uow = _unitOfWorkManager.Begin())
            {
                result = await _airplaneManager.GetByIdAsync(99999);
                await uow.CompleteAsync();
            }

            result.ShouldBeNull();
        }

        [Fact]
        public async Task Dado_AirplaneManager_Quando_AtualizarAeronave_Entao_DeveRefletirAlteracoes()
        {
            int createdId;
            using (var uow = _unitOfWorkManager.Begin())
            {
                var airplane = new Airplane
                {
                    Number = "UPD-001",
                    Model = "Embraer E190",
                    TenantId = AbpSession.TenantId
                };
                var created = await _airplaneManager.CreateAsync(airplane);
                createdId = created.Id;
                await uow.CompleteAsync();
            }

            Airplane updated;
            using (var uow = _unitOfWorkManager.Begin())
            {
                var toUpdate = await _airplaneManager.GetByIdAsync(createdId);
                toUpdate.Model = "Embraer E195-E2";
                updated = await _airplaneManager.UpdateAsync(toUpdate);
                await uow.CompleteAsync();
            }

            updated.ShouldNotBeNull();
            updated.Model.ShouldBe("Embraer E195-E2");
            updated.Number.ShouldBe("UPD-001");
        }

        [Fact]
        public async Task Dado_AirplaneManager_Quando_DeletarAeronave_Entao_NaoDeveSerEncontrada()
        {
            int createdId;
            using (var uow = _unitOfWorkManager.Begin())
            {
                var airplane = new Airplane
                {
                    Number = "DEL-001",
                    Model = "Boeing 777",
                    TenantId = AbpSession.TenantId
                };
                var created = await _airplaneManager.CreateAsync(airplane);
                createdId = created.Id;
                await uow.CompleteAsync();
            }

            using (var uow = _unitOfWorkManager.Begin())
            {
                await _airplaneManager.DeleteAsync(createdId);
                await uow.CompleteAsync();
            }

            Airplane result;
            using (var uow = _unitOfWorkManager.Begin())
            {
                result = await _airplaneManager.GetByIdAsync(createdId);
                await uow.CompleteAsync();
            }
            result.ShouldBeNull();
        }

        [Fact]
        public async Task Dado_AirplaneManager_Quando_CriarAeronaveComNumeroDuplicado_Entao_DeveLancarExcecao()
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                var airplane1 = new Airplane
                {
                    Number = "dup-001",
                    Model = "Boeing 737",
                    TenantId = AbpSession.TenantId
                };
                await _airplaneManager.CreateAsync(airplane1);
                await uow.CompleteAsync();
            }

            await Should.ThrowAsync<Abp.UI.UserFriendlyException>(async () =>
            {
                using (var uow = _unitOfWorkManager.Begin())
                {
                    var airplane2 = new Airplane
                    {
                        Number = "dup-001",
                        Model = "Airbus A380",
                        TenantId = AbpSession.TenantId
                    };
                    await _airplaneManager.CreateAsync(airplane2);
                    await uow.CompleteAsync();
                }
            });
        }

        [Fact]
        public async Task Dado_AirplaneManager_Quando_ConsultarAirplanes_Entao_DeveRetornarQueryable()
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                var airplanes = _airplaneManager.Airplanes;
                airplanes.ShouldNotBeNull();
                await uow.CompleteAsync();
            }
        }

        [Fact]
        public async Task Dado_AirplaneManager_Quando_CriarMultiplasAeronaves_Entao_TodasDevemSerListadas()
        {
            int countBefore;
            using (var uow = _unitOfWorkManager.Begin())
            {
                countBefore = _airplaneManager.Airplanes.Count();
                await uow.CompleteAsync();
            }

            using (var uow = _unitOfWorkManager.Begin())
            {
                await _airplaneManager.CreateAsync(new Airplane
                {
                    Number = "MULTI-001",
                    Model = "Boeing 747",
                    TenantId = AbpSession.TenantId
                });
                await _airplaneManager.CreateAsync(new Airplane
                {
                    Number = "MULTI-002",
                    Model = "Boeing 787",
                    TenantId = AbpSession.TenantId
                });
                await uow.CompleteAsync();
            }

            int countAfter;
            using (var uow = _unitOfWorkManager.Begin())
            {
                countAfter = _airplaneManager.Airplanes.Count();
                await uow.CompleteAsync();
            }

            countAfter.ShouldBe(countBefore + 2);
        }
    }
}
