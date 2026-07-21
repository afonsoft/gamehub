using Abp.Configuration;
using Abp.Configuration.Startup;
using System.Collections.Generic;
using System.Linq;

namespace GameHub.EntityHistory
{
    public class EntityHistoryConfigProvider : ICustomConfigProvider
    {
        private readonly IAbpStartupConfiguration _eafStartupConfiguration;

        public EntityHistoryConfigProvider(
            IAbpStartupConfiguration eafStartupConfiguration
        )
        {
            _eafStartupConfiguration = eafStartupConfiguration;
        }

        public Dictionary<string, object> GetConfig(CustomConfigProviderContext customConfigProviderContext)
        {
            if (!_eafStartupConfiguration.EntityHistory.IsEnabled)
            {
                return new Dictionary<string, object>
                {
                    { "EntityHistory", new { IsEnabled = false }}
                };
            }

            var entityHistoryEnabledEntities = EntityHistoryHelper.TrackedTypes
                .Where(type => _eafStartupConfiguration.EntityHistory.Selectors.Any(s => s.Predicate(type)))
                .Select(type => type.FullName)
                .ToList();

            return new Dictionary<string, object>
            {
                { "EntityHistory", new { IsEnabled = true, EnabledEntities = entityHistoryEnabledEntities }}
            };
        }
    }
}