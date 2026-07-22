namespace GameHub
{
    public static class GameHubConsts
    {
        public const string DbTablePrefix = "gh_";

        public const string DbSchema = null;

        public const int MaxSlugLength = 256;

        public const int MaxDescriptionLength = 4000;

        public const long MaxBuildPackageSizeBytes = 100L * 1024 * 1024;

        public const string LocalizationSourceName = "GameHub";

        public const string ConnectionStringName = "Default";

        public const string DefaultCorsPolicyName = "GameHubCorsPolicy";

        public const bool MultiTenancyEnabled = true;
    }
}
