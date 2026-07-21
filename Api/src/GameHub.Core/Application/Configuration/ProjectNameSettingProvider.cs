using Abp.Configuration;
using System.Collections.Generic;

namespace GameHub.Configuration
{
    public class ProjectNameSettingProvider : SettingProvider
    {
        public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
        {
            return new[]
                {
                    new SettingDefinition(ProjectNameSettings.AirplaneSettings.IsAirplaneManagerEnabled, "true"),
                };
        }
    }
}