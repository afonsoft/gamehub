using System;
using GameHub.Playtesting;

namespace GameHub.Playtesting.Dto
{
    public class PlaytestSessionDto
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public long RequestedByUserId { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public string RecordingUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
