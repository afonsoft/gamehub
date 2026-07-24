using System;

namespace GameHub.Privacy.Dto
{
    /// <summary>Input for saving a player's privacy consent.</summary>
    public class SavePrivacyConsentInput
    {
        /// <summary>Game identifier.</summary>
        public Guid GameId { get; set; }

        /// <summary>Version or timestamp of the accepted policy.</summary>
        public string PolicyVersion { get; set; } = string.Empty;
    }
}
