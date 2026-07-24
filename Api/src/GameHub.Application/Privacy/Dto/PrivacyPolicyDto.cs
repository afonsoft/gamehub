namespace GameHub.Privacy.Dto
{
    /// <summary>Privacy policy information for a game.</summary>
    public class PrivacyPolicyDto
    {
        /// <summary>Game identifier.</summary>
        public string GameSlug { get; set; } = string.Empty;

        /// <summary>Hosted privacy policy URL.</summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>Plain text summary of the privacy policy.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Whether the game requires external requests.</summary>
        public bool RequiresConsent { get; set; }
    }
}
