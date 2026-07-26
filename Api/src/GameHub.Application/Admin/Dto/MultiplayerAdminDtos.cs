using System;
using GameHub.Multiplayer;

namespace GameHub.Admin.Dto
{
    public class MultiplayerAdminMatchDto
    {
        public Guid MatchId { get; set; }
        public Guid GameId { get; set; }
        public string RoomCode { get; set; }
        public string Mode { get; set; }
        public MatchStatus Status { get; set; }
        public int ParticipantCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MultiplayerSecurityEventDto
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public Guid? MatchId { get; set; }
        public long? UserId { get; set; }
        public string EventType { get; set; }
        public string Reason { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
