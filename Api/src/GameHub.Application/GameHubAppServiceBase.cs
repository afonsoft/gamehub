using Eaf.Middleware;

namespace GameHub
{
    public abstract class GameHubAppServiceBase : MiddlewareAppServiceBase
    {
        /* ADD YOUR COMMON MEMBERS FOR ALL YOUR APP SERVICES. */

        protected GameHubAppServiceBase()
        {
            LocalizationSourceName = GameHubConsts.LocalizationSourceName;
        }
    }
}