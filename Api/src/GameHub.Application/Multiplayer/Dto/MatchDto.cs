using System;
using System.Collections.Generic;

namespace GameHub.Multiplayer.Dto
{
    public class MatchDto
    {
        public Guid Id { get; set; }

        public Guid GameId { get; set; }

        public string RoomCode { get; set; } = string.Empty;

        public string Mode { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int MaxPlayers { get; set; }

        public int MaxSpectators { get; set; }

        public string PayloadJson { get; set; } = string.Empty;

        public DateTime? ExpiresAt { get; set; }

        public List<MatchParticipantDto> Participants { get; set; } = new();
    }

    public class MatchParticipantDto
    {
        public Guid Id { get; set; }

        public long? UserId { get; set; }

        public string AnonymousIdHash { get; set; } = string.Empty;

        public string ConnectionId { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool IsSpectator { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}
