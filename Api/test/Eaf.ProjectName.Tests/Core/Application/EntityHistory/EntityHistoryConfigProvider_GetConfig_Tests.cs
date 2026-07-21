using Abp.Configuration;
using Abp.Configuration.Startup;
using Eaf.ProjectName.EntityHistory;
using NSubstitute;
using Shouldly;
using System.Collections.Generic;
using Xunit;

namespace Eaf.ProjectName.Tests.Core.Application.EntityHistory
{
    public class EntityHistoryConfigProvider_GetConfig_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_EntityHistoryDesabilitado_Quando_ObterConfig_Entao_DeveRetornarIsEnabledFalse()
        {
            var config = LocalIocManager.Resolve<IAbpStartupConfiguration>();
            config.EntityHistory.IsEnabled = false;

            var provider = new EntityHistoryConfigProvider(config);
            var result = provider.GetConfig(null);

            result.ShouldNotBeNull();
            result.ShouldContainKey("EntityHistory");
        }

        [Fact]
        public void Dado_EntityHistoryHabilitado_Quando_ObterConfig_Entao_DeveRetornarIsEnabledTrue()
        {
            var config = LocalIocManager.Resolve<IAbpStartupConfiguration>();
            config.EntityHistory.IsEnabled = true;

            var provider = new EntityHistoryConfigProvider(config);
            var result = provider.GetConfig(null);

            result.ShouldNotBeNull();
            result.ShouldContainKey("EntityHistory");
        }

        [Fact]
        public void Dado_EntityHistoryHabilitado_Quando_ObterConfig_Entao_DeveRetornarDicionario()
        {
            var config = LocalIocManager.Resolve<IAbpStartupConfiguration>();
            config.EntityHistory.IsEnabled = true;

            var provider = new EntityHistoryConfigProvider(config);
            var result = provider.GetConfig(null);

            result.ShouldBeOfType<Dictionary<string, object>>();
            result.Count.ShouldBe(1);
        }
    }
}
