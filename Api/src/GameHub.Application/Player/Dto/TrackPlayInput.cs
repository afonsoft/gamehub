using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Player.Dto
{
    /// <summary>
    /// Input to track a recent play.
    /// </summary>
    public class TrackPlayInput
    {
        [Required]
        public Guid GameId { get; set; }
    }
}
