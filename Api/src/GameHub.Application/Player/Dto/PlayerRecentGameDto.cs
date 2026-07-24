using System;
using GameHub.Catalog.Dto;

namespace GameHub.Player.Dto
{
    /// <summary>
    /// A recently played game entry for the player.
    /// </summary>
    public class PlayerRecentGameDto
    {
        public Guid GameId { get; set; }

        public GameCardDto Game { get; set; }

        public DateTime LastPlayedAt { get; set; }

        public long TotalSessions { get; set; }
    }
}
