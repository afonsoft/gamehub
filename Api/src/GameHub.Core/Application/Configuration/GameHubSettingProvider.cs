using Abp.Configuration;
using System.Collections.Generic;

namespace GameHub.Configuration
{
    public class GameHubSettingProvider : SettingProvider
    {
        public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
        {
            return new[]
                {
                    new SettingDefinition(GameHubSettings.General.PlatformName, "GameHub"),
                };
        }
    }
}