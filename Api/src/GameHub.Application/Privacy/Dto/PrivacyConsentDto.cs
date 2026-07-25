using System;

namespace GameHub.Privacy.Dto
{
    /// <summary>Represents a player's consent status for a game's privacy policy.</summary>
    public class PrivacyConsentDto
    {
        /// <summary>Game identifier.</summary>
        public Guid GameId { get; set; }

        /// <summary>Whether the player has consented to the current policy.</summary>
        public bool Consented { get; set; }

        /// <summary>Version or timestamp of the accepted policy.</summary>
        public string PolicyVersion { get; set; } = string.Empty;

        /// <summary>When the consent was recorded, if applicable.</summary>
        public DateTime? ConsentedAt { get; set; }
    }
}
