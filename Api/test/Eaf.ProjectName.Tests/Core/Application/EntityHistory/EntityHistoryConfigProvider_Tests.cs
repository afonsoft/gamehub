using Eaf.ProjectName.EntityHistory;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Core.Application.EntityHistory
{
    public class EntityHistoryConfigProvider_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_EntityHistoryConfigProvider_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            var configuration = LocalIocManager.Resolve<Abp.Configuration.Startup.IAbpStartupConfiguration>();
            var provider = new EntityHistoryConfigProvider(configuration);
            provider.ShouldNotBeNull();
        }
    }
}
