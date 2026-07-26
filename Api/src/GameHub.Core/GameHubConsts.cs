namespace GameHub
{
    public static class GameHubConsts
    {
        public const string DbTablePrefix = "gh_";

        public const string DbSchema = null;

        public const int MaxSlugLength = 256;

        public const int MaxDescriptionLength = 4000;

        public const long MaxBuildPackageSizeBytes = 100L * 1024 * 1024;

        public const long BuildPackageWarningSizeBytes = 8L * 1024 * 1024;

        public const long LargeFileWarningSizeBytes = 1L * 1024 * 1024;

        /// <summary>Image assets larger than this threshold trigger an optimization warning.</summary>
        public const long ImageOptimizationWarningSizeBytes = 100L * 1024;

        /// <summary>Estimated gross revenue per commercial break (USD).</summary>
        public const decimal EstimatedCommercialBreakRevenue = 0.002m;

        /// <summary>Estimated gross revenue per rewarded break completion (USD).</summary>
        public const decimal EstimatedRewardedBreakRevenue = 0.01m;

        public const string LocalizationSourceName = "GameHub";

        public const string ConnectionStringName = "Default";

        public const string DefaultCorsPolicyName = "GameHubCorsPolicy";

        public const bool MultiTenancyEnabled = true;
    }
}
