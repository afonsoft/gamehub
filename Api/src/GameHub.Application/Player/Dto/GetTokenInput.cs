using System;

namespace GameHub.Player.Dto
{
    /// <summary>Input for requesting a short-lived game token.</summary>
    public class GetTokenInput
    {
        /// <summary>Game identifier to scope the token.</summary>
        public Guid GameId { get; set; }
    }
}
