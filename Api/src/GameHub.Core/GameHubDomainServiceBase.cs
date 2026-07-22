using Abp.Domain.Services;

namespace GameHub
{
    public abstract class GameHubDomainServiceBase : DomainService
    {
        /* ADD YOUR COMMON MEMBERS FOR ALL YOUR DOMAIN SERVICES. */

        protected GameHubDomainServiceBase()
        {
            LocalizationSourceName = GameHubConsts.LocalizationSourceName;
        }
    }
}