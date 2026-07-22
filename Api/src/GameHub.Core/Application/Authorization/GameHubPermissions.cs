namespace GameHub.Authorization
{
    /// <summary>
    /// Permissões da plataforma GameHub.
    /// </summary>
    public static class GameHubPermissions
    {
        // Games
        public const string Pages_Games = "Pages.Games";
        public const string Pages_Games_View = "Pages.Games.View";
        public const string Pages_Games_Create = "Pages.Games.Create";
        public const string Pages_Games_Edit = "Pages.Games.Edit";
        public const string Pages_Games_Delete = "Pages.Games.Delete";
        public const string Pages_Games_Publish = "Pages.Games.Publish";
        public const string Pages_Games_Suspend = "Pages.Games.Suspend";

        // Builds
        public const string Pages_Builds = "Pages.Builds";
        public const string Pages_Builds_Upload = "Pages.Builds.Upload";
        public const string Pages_Builds_View = "Pages.Builds.View";
        public const string Pages_Builds_Approve = "Pages.Builds.Approve";
        public const string Pages_Builds_Reject = "Pages.Builds.Reject";

        // Moderation
        public const string Pages_Moderation = "Pages.Moderation";
        public const string Pages_Moderation_View = "Pages.Moderation.View";
        public const string Pages_Moderation_Review = "Pages.Moderation.Review";
        public const string Pages_Moderation_Complete = "Pages.Moderation.Complete";

        // Categories
        public const string Pages_Categories = "Pages.Categories";
        public const string Pages_Categories_Manage = "Pages.Categories.Manage";

        // Tags
        public const string Pages_Tags = "Pages.Tags";
        public const string Pages_Tags_Manage = "Pages.Tags.Manage";

        // Dashboard / Admin
        public const string Pages_Dashboard = "Pages.GameHubDashboard";
        public const string Pages_Dashboard_View = "Pages.GameHubDashboard.View";
        public const string Pages_Dashboard_FeatureFlags = "Pages.GameHubDashboard.FeatureFlags";
        public const string Pages_Dashboard_AuditLog = "Pages.GameHubDashboard.AuditLog";

        // Reports
        public const string Pages_Reports = "Pages.Reports";
        public const string Pages_Reports_View = "Pages.Reports.View";
        public const string Pages_Reports_Manage = "Pages.Reports.Manage";

        // Developer
        public const string Pages_Developer = "Pages.Developer";
        public const string Pages_Developer_Profile = "Pages.Developer.Profile";
        public const string Pages_Developer_Games = "Pages.Developer.Games";

        // Gameplay
        public const string Pages_Gameplay = "Pages.Gameplay";
        public const string Pages_Leaderboard = "Pages.Leaderboard";
    }
}
