using Abp.Domain.Uow;
using GameHub.Airplanes;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace GameHub.Tests.Airplanes
{
    public class AirplanesAppService_StartJob_Tests : GameHubTestBase
    {
        private readonly IAirplanesAppService _airplanesAppService;

        public AirplanesAppService_StartJob_Tests()
        {
            _airplanesAppService = LocalIocManager.Resolve<IAirplanesAppService>();
        }

        [Fact]
        public async Task Dado_AirplanesAppService_Quando_IniciarJob_Entao_DeveExecutarSemExcecao()
        {
            await Should.NotThrowAsync(async () => await _airplanesAppService.StartJob());
        }
    }
}
