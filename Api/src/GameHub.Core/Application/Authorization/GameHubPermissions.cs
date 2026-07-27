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

        // Users
        public const string Pages_Users = "Pages.Users";
        public const string Pages_Users_Manage = "Pages.Users.Manage";

        // Reports
        public const string Pages_Reports = "Pages.Reports";
        public const string Pages_Reports_View = "Pages.Reports.View";
        public const string Pages_Reports_Manage = "Pages.Reports.Manage";

        // Developer
        public const string Pages_Developer = "Pages.Developer";
        public const string Pages_Developer_Profile = "Pages.Developer.Profile";
        public const string Pages_Developer_Games = "Pages.Developer.Games";

        // Companies
        public const string Pages_Companies = "Pages.Companies";
        public const string Pages_Companies_Manage = "Pages.Companies.Manage";
        public const string Pages_Company_Employees = "Pages.Company.Employees";
        public const string Pages_Company_Employees_Manage = "Pages.Company.Employees.Manage";

        // Gameplay
        public const string Pages_Gameplay = "Pages.Gameplay";
        public const string Pages_Leaderboard = "Pages.Leaderboard";
        public const string Pages_Multiplayer = "Pages.Multiplayer";
        public const string Pages_Multiplayer_Manage = "Pages.Multiplayer.Manage";

        /// <summary>
        /// Conjunto completo de permissões para administradores host.
        /// </summary>
        public static string[] AllPermissions() => new[]
        {
            Pages_Games,
            Pages_Games_View,
            Pages_Games_Create,
            Pages_Games_Edit,
            Pages_Games_Delete,
            Pages_Games_Publish,
            Pages_Games_Suspend,
            Pages_Builds,
            Pages_Builds_Upload,
            Pages_Builds_View,
            Pages_Builds_Approve,
            Pages_Builds_Reject,
            Pages_Moderation,
            Pages_Moderation_View,
            Pages_Moderation_Review,
            Pages_Moderation_Complete,
            Pages_Categories,
            Pages_Categories_Manage,
            Pages_Tags,
            Pages_Tags_Manage,
            Pages_Dashboard,
            Pages_Dashboard_View,
            Pages_Dashboard_FeatureFlags,
            Pages_Dashboard_AuditLog,
            Pages_Users,
            Pages_Users_Manage,
            Pages_Reports,
            Pages_Reports_View,
            Pages_Reports_Manage,
            Pages_Developer,
            Pages_Developer_Profile,
            Pages_Developer_Games,
            Pages_Companies,
            Pages_Companies_Manage,
            Pages_Company_Employees,
            Pages_Company_Employees_Manage,
            Pages_Gameplay,
            Pages_Leaderboard,
            Pages_Multiplayer,
            Pages_Multiplayer_Manage
        };

        /// <summary>
        /// Conjunto padrão de permissões para administradores.
        /// </summary>
        public static string[] AdminPermissions() => new[]
        {
            Pages_Games,
            Pages_Games_View,
            Pages_Games_Publish,
            Pages_Games_Suspend,
            Pages_Builds,
            Pages_Builds_View,
            Pages_Builds_Approve,
            Pages_Builds_Reject,
            Pages_Moderation,
            Pages_Moderation_View,
            Pages_Categories,
            Pages_Categories_Manage,
            Pages_Tags,
            Pages_Tags_Manage,
            Pages_Dashboard,
            Pages_Dashboard_View,
            Pages_Dashboard_FeatureFlags,
            Pages_Dashboard_AuditLog,
            Pages_Developer,
            Pages_Developer_Profile,
            Pages_Developer_Games,
            Pages_Companies,
            Pages_Companies_Manage,
            Pages_Company_Employees,
            Pages_Company_Employees_Manage,
            Pages_Users,
            Pages_Users_Manage,
            Pages_Gameplay,
            Pages_Leaderboard,
            Pages_Multiplayer,
            Pages_Multiplayer_Manage
        };

        /// <summary>
        /// Conjunto padrão de permissões para moderadores.
        /// </summary>
        public static string[] ModeratorPermissions() => new[]
        {
            Pages_Games_View,
            Pages_Builds,
            Pages_Builds_View,
            Pages_Builds_Approve,
            Pages_Builds_Reject,
            Pages_Moderation,
            Pages_Moderation_View,
            Pages_Moderation_Review,
            Pages_Moderation_Complete,
            Pages_Gameplay,
            Pages_Leaderboard
        };

        /// <summary>
        /// Conjunto padrão de permissões para desenvolvedores/funcionários de empresas.
        /// </summary>
        public static string[] DeveloperPermissions() => new[]
        {
            Pages_Games,
            Pages_Games_View,
            Pages_Games_Create,
            Pages_Games_Edit,
            Pages_Games_Delete,
            Pages_Builds,
            Pages_Builds_Upload,
            Pages_Builds_View,
            Pages_Developer,
            Pages_Developer_Profile,
            Pages_Developer_Games,
            Pages_Company_Employees,
            Pages_Gameplay,
            Pages_Leaderboard
        };

        /// <summary>
        /// Conjunto padrão de permissões para jogadores.
        /// </summary>
        public static string[] PlayerPermissions() => new[]
        {
            Pages_Games_View,
            Pages_Gameplay,
            Pages_Leaderboard
        };
    }
}
