using Abp.Application.Services.Dto;
using Abp.Authorization;
using Eaf.ProjectName.Airplanes;
using Eaf.ProjectName.Airplanes.Dtos;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Airplanes
{
    public class AirplanesAppService_Tests : ProjectNameTestBase
    {
        private readonly AirplanesAppService _airplanesAppService;

        public AirplanesAppService_Tests()
        {
            _airplanesAppService = LocalIocManager.Resolve<AirplanesAppService>();
            _airplanesAppService.AbpSession = AbpSession;
        }

        [Fact]
        public void Dado_AirplanesAppService_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            _airplanesAppService.ShouldNotBeNull();
        }
    }
}
