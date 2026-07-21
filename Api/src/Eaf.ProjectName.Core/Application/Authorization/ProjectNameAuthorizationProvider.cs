using Abp;
using Abp.Authorization;
using Abp.Localization;
using Eaf.Middleware.Authorization;

namespace Eaf.ProjectName.Authorization
{
    public class ProjectNameAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            var pages = context.GetPermissionOrNull(MiddlewarePermissions.Pages) ?? context.CreatePermission(MiddlewarePermissions.Pages, LEaf("Pages"));

            var airplanes = pages.CreateChildPermission(ProjectNamePermissions.Pages_Airplanes, L("Airplanes"));
            airplanes.CreateChildPermission(ProjectNamePermissions.Pages_Airplanes_Create, L("CreateNewAirplane"));
            airplanes.CreateChildPermission(ProjectNamePermissions.Pages_Airplanes_Edit, L("EditAirplane"));
            airplanes.CreateChildPermission(ProjectNamePermissions.Pages_Airplanes_Delete, L("DeleteAirplane"));
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, ProjectNameConsts.LocalizationSourceName);
        }

        private static ILocalizableString LEaf(string name)
        {
            return new LocalizableString(name, AbpConsts.LocalizationSourceName);
        }
    }
}