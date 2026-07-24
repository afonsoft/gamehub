using System;
using GameHub.Catalog.Dto;

namespace GameHub.Player.Dto
{
    /// <summary>
    /// A favorite game entry for the player.
    /// </summary>
    public class PlayerFavoriteDto
    {
        public Guid GameId { get; set; }

        public GameCardDto Game { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
