using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Privacy.Dto
{
    /// <summary>Input for retrieving a player's privacy consent for a game.</summary>
    public class GetPrivacyConsentInput
    {
        /// <summary>Game identifier.</summary>
        [Required]
        public Guid GameId { get; set; }
    }
}
