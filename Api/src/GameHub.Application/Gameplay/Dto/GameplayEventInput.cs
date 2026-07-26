using System;
using System.ComponentModel.DataAnnotations;
using GameHub;

namespace GameHub.Gameplay.Dto
{
    /// <summary>
    /// A single gameplay event from the Game SDK.
    /// </summary>
    public class GameplayEventInput
    {
        /// <summary>Session identifier.</summary>
        [Required]
        public Guid SessionId { get; set; }

        /// <summary>Game identifier (redundante, mas usado para rastreamento).</summary>
        [Required]
        public Guid GameId { get; set; }

        /// <summary>Build identifier associated with the event.</summary>
        public Guid? BuildId { get; set; }

        /// <summary>Match identifier associated with the event.</summary>
        public Guid? MatchId { get; set; }

        /// <summary>Type of gameplay event.</summary>
        [Required]
        public GameplayEventType EventType { get; set; }

        /// <summary>Event name (e.g., "level_complete").</summary>
        [StringLength(100)]
        public string EventName { get; set; }

        /// <summary>Arbitrary JSON payload.</summary>
        [StringLength(4096)]
        public string PayloadJson { get; set; }
    }
}
