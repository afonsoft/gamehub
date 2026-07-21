using Eaf.Middleware;

namespace GameHub
{
    public abstract class ProjectNameAppServiceBase : MiddlewareAppServiceBase
    {
        /* ADD YOUR COMMON MEMBERS FOR ALL YOUR APP SERVICES. */

        protected ProjectNameAppServiceBase()
        {
            LocalizationSourceName = ProjectNameConsts.LocalizationSourceName;
        }
    }
}