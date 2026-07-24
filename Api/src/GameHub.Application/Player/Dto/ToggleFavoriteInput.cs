using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Player.Dto
{
    /// <summary>
    /// Input to add or remove a favorite game.
    /// </summary>
    public class ToggleFavoriteInput
    {
        [Required]
        public Guid GameId { get; set; }
    }
}
