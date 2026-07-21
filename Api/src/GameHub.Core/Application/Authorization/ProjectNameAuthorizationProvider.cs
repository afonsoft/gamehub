using Abp;
using Abp.Authorization;
using Abp.Localization;
using Eaf.Middleware.Authorization;

namespace GameHub.Authorization
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

            RegisterGameHubPermissions(pages);
        }

        private static void RegisterGameHubPermissions(Permission pages)
        {
            var games = pages.CreateChildPermission(GameHubPermissions.Pages_Games, L("Games"));
            games.CreateChildPermission(GameHubPermissions.Pages_Games_View, L("View"));
            games.CreateChildPermission(GameHubPermissions.Pages_Games_Create, L("Create"));
            games.CreateChildPermission(GameHubPermissions.Pages_Games_Edit, L("Edit"));
            games.CreateChildPermission(GameHubPermissions.Pages_Games_Delete, L("Delete"));
            games.CreateChildPermission(GameHubPermissions.Pages_Games_Publish, L("Publish"));
            games.CreateChildPermission(GameHubPermissions.Pages_Games_Suspend, L("Suspend"));

            var builds = pages.CreateChildPermission(GameHubPermissions.Pages_Builds, L("Builds"));
            builds.CreateChildPermission(GameHubPermissions.Pages_Builds_Upload, L("Upload"));
            builds.CreateChildPermission(GameHubPermissions.Pages_Builds_View, L("View"));
            builds.CreateChildPermission(GameHubPermissions.Pages_Builds_Approve, L("Approve"));
            builds.CreateChildPermission(GameHubPermissions.Pages_Builds_Reject, L("Reject"));

            var moderation = pages.CreateChildPermission(GameHubPermissions.Pages_Moderation, L("Moderation"));
            moderation.CreateChildPermission(GameHubPermissions.Pages_Moderation_View, L("View"));
            moderation.CreateChildPermission(GameHubPermissions.Pages_Moderation_Review, L("Review"));
            moderation.CreateChildPermission(GameHubPermissions.Pages_Moderation_Complete, L("Complete"));

            var categories = pages.CreateChildPermission(GameHubPermissions.Pages_Categories, L("Categories"));
            categories.CreateChildPermission(GameHubPermissions.Pages_Categories_Manage, L("Manage"));

            var tags = pages.CreateChildPermission(GameHubPermissions.Pages_Tags, L("Tags"));
            tags.CreateChildPermission(GameHubPermissions.Pages_Tags_Manage, L("Manage"));

            var dashboard = pages.CreateChildPermission(GameHubPermissions.Pages_Dashboard, L("Dashboard"));
            dashboard.CreateChildPermission(GameHubPermissions.Pages_Dashboard_View, L("View"));
            dashboard.CreateChildPermission(GameHubPermissions.Pages_Dashboard_FeatureFlags, L("FeatureFlags"));
            dashboard.CreateChildPermission(GameHubPermissions.Pages_Dashboard_AuditLog, L("AuditLog"));

            var reports = pages.CreateChildPermission(GameHubPermissions.Pages_Reports, L("Reports"));
            reports.CreateChildPermission(GameHubPermissions.Pages_Reports_View, L("View"));
            reports.CreateChildPermission(GameHubPermissions.Pages_Reports_Manage, L("Manage"));

            var developer = pages.CreateChildPermission(GameHubPermissions.Pages_Developer, L("Developer"));
            developer.CreateChildPermission(GameHubPermissions.Pages_Developer_Profile, L("Profile"));
            developer.CreateChildPermission(GameHubPermissions.Pages_Developer_Games, L("Games"));

            pages.CreateChildPermission(GameHubPermissions.Pages_Gameplay, L("Gameplay"));
            pages.CreateChildPermission(GameHubPermissions.Pages_Leaderboard, L("Leaderboard"));
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