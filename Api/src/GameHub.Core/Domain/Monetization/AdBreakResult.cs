namespace GameHub.Monetization
{
    /// <summary>
    /// Result of an ad break interaction.
    /// </summary>
    public class AdBreakResult
    {
        public bool Completed { get; set; }

        public int AdDurationSeconds { get; set; }

        public bool RewardGranted { get; set; }

        public bool AdBlocked { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
