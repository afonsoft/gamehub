using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Multiplayer.Dto
{
    public class CreateMatchInput
    {
        [Required]
        public Guid GameId { get; set; }

        [StringLength(64)]
        public string Mode { get; set; } = "default";

        [Range(2, 64)]
        public int? MaxPlayers { get; set; }
    }

    public class JoinMatchInput
    {
        [Required]
        public Guid MatchId { get; set; }

        public long? UserId { get; set; }

        [StringLength(128)]
        public string AnonymousIdHash { get; set; } = string.Empty;

        [StringLength(256)]
        public string ConnectionId { get; set; } = string.Empty;
    }

    public class JoinMatchByRoomCodeInput
    {
        [Required]
        [StringLength(16)]
        public string RoomCode { get; set; } = string.Empty;

        public long? UserId { get; set; }

        [StringLength(128)]
        public string AnonymousIdHash { get; set; } = string.Empty;

        [StringLength(256)]
        public string ConnectionId { get; set; } = string.Empty;
    }

    public class LeaveMatchInput
    {
        [Required]
        public Guid MatchId { get; set; }

        [Required]
        [StringLength(256)]
        public string ConnectionId { get; set; } = string.Empty;
    }

    public class UpdateMatchStateInput
    {
        [Required]
        public Guid MatchId { get; set; }

        public string PayloadJson { get; set; } = string.Empty;
    }
}
