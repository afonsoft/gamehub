namespace GameHub.Configuration
{
    /// <summary>
    /// Options for the static VAST ad provider example.
    /// </summary>
    public class StaticVastAdOptions
    {
        /// <summary>Default simulated ad duration in seconds.</summary>
        public int DefaultDurationSeconds { get; set; } = 15;

        /// <summary>URL of a static VAST/MP4 resource used as reference.</summary>
        public string StaticMediaUrl { get; set; } = string.Empty;

        /// <summary>When true, simulates an ad-blocked environment.</summary>
        public bool SimulateAdBlocked { get; set; } = false;
    }
}
