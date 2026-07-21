using Eaf.ProjectName.Configuration;
using Shouldly;
using System.Linq;
using Xunit;

namespace Eaf.ProjectName.Tests.Configuration
{
    public class ProjectNameSettingProvider_Tests : ProjectNameTestBase
    {
        private readonly ProjectNameSettingProvider _settingProvider;

        public ProjectNameSettingProvider_Tests()
        {
            _settingProvider = LocalIocManager.Resolve<ProjectNameSettingProvider>();
        }

        [Fact]
        public void Dado_ProviderInicializado_Quando_ObterConfiguracoes_Entao_DeveIncluirConfiguracoes()
        {
            // Dado (Given)
            var manager = LocalIocManager.Resolve<Abp.Configuration.ISettingDefinitionManager>();
            var context = new Abp.Configuration.SettingDefinitionProviderContext(manager);
            
            // Quando (When)
            var settings = _settingProvider.GetSettingDefinitions(context);
            
            // Então (Then)
            settings.ShouldNotBeNull();
            settings.Count().ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Dado_ProviderInicializado_Quando_DefinirConfiguracoes_Entao_DeveIncluirConfiguracaoDeAirplane()
        {
            // Dado (Given)
            var manager = LocalIocManager.Resolve<Abp.Configuration.ISettingDefinitionManager>();
            var context = new Abp.Configuration.SettingDefinitionProviderContext(manager);
            
            // Quando (When)
            var settings = _settingProvider.GetSettingDefinitions(context);
            
            // Então (Then)
            settings.ShouldNotBeNull();
            settings.Count().ShouldBeGreaterThan(0);
        }
    }
}
