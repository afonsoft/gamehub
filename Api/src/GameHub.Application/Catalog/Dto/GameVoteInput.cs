using System;
using System.ComponentModel.DataAnnotations;
using GameHub.Catalog;

namespace GameHub.Catalog.Dto
{
    /// <summary>
    /// Input for registering a like or dislike vote on a game.
    /// </summary>
    public class GameVoteInput
    {
        [Required]
        public Guid GameId { get; set; }

        [Required]
        public GameVoteType VoteType { get; set; }

        /// <summary>
        /// Client-generated fingerprint required for anonymous users.
        /// </summary>
        [StringLength(64)]
        public string DeviceId { get; set; }
    }
}
