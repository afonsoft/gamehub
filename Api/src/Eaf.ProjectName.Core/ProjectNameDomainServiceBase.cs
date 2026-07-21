using Abp.Domain.Services;

namespace Eaf.ProjectName
{
    public abstract class ProjectNameDomainServiceBase : DomainService
    {
        /* ADD YOUR COMMON MEMBERS FOR ALL YOUR DOMAIN SERVICES. */

        protected ProjectNameDomainServiceBase()
        {
            LocalizationSourceName = ProjectNameConsts.LocalizationSourceName;
        }
    }
}