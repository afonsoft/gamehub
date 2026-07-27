using Abp;
using Abp.Authorization;
using Abp.Localization;
using Eaf.Middleware.Authorization;

namespace GameHub.Authorization
{
    public class GameHubAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            var pages = context.GetPermissionOrNull(MiddlewarePermissions.Pages) ?? context.CreatePermission(MiddlewarePermissions.Pages, LEaf("Pages"));

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

            var users = pages.CreateChildPermission(GameHubPermissions.Pages_Users, L("Users"));
            users.CreateChildPermission(GameHubPermissions.Pages_Users_Manage, L("Manage"));

            var reports = pages.CreateChildPermission(GameHubPermissions.Pages_Reports, L("Reports"));
            reports.CreateChildPermission(GameHubPermissions.Pages_Reports_View, L("View"));
            reports.CreateChildPermission(GameHubPermissions.Pages_Reports_Manage, L("Manage"));

            var developer = pages.CreateChildPermission(GameHubPermissions.Pages_Developer, L("Developer"));
            developer.CreateChildPermission(GameHubPermissions.Pages_Developer_Profile, L("Profile"));
            developer.CreateChildPermission(GameHubPermissions.Pages_Developer_Games, L("Games"));

            var companies = pages.CreateChildPermission(GameHubPermissions.Pages_Companies, L("Companies"));
            companies.CreateChildPermission(GameHubPermissions.Pages_Companies_Manage, L("Manage"));
            var companyEmployees = companies.CreateChildPermission(GameHubPermissions.Pages_Company_Employees, L("Employees"));
            companyEmployees.CreateChildPermission(GameHubPermissions.Pages_Company_Employees_Manage, L("Manage"));

            pages.CreateChildPermission(GameHubPermissions.Pages_Gameplay, L("Gameplay"));
            pages.CreateChildPermission(GameHubPermissions.Pages_Leaderboard, L("Leaderboard"));
            var multiplayer = pages.CreateChildPermission(GameHubPermissions.Pages_Multiplayer, L("Multiplayer"));
            multiplayer.CreateChildPermission(GameHubPermissions.Pages_Multiplayer_Manage, L("Manage"));
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, GameHubConsts.LocalizationSourceName);
        }

        private static ILocalizableString LEaf(string name)
        {
            return new LocalizableString(name, AbpConsts.LocalizationSourceName);
        }
    }
}