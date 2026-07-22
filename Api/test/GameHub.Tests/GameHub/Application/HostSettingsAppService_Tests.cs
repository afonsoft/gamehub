using Eaf.Middleware.Configuration.Host;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class HostSettingsAppService_Tests : GameHubTestBase
    {
        private readonly IHostSettingsAppService _hostSettingsAppService;

        public HostSettingsAppService_Tests()
        {
            _hostSettingsAppService = Resolve<IHostSettingsAppService>();
        }

        [Fact]
        public async Task Deve_Retornar_Configuracoes_Anonimamente()
        {
            var result = await _hostSettingsAppService.GetAllSettingsAnonymous();
            result.ShouldNotBeNull();
        }
    }
}
