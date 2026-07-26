using System;
using System.Collections.Generic;

namespace GameHub.Multiplayer.Dto
{
    public class MatchBrowserDto
    {
        public Guid MatchId { get; set; }
        public Guid GameId { get; set; }
        public string RoomCode { get; set; }
        public string Mode { get; set; }
        public string Region { get; set; }
        public int Players { get; set; }
        public int Spectators { get; set; }
        public int MaxPlayers { get; set; }
        public int? AverageLatencyMs { get; set; }
        public bool IsRanked { get; set; }
        public MatchStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RankedQueueDto
    {
        public Guid QueueEntryId { get; set; }
        public Guid GameId { get; set; }
        public Guid SeasonId { get; set; }
        public string Mode { get; set; }
        public string Region { get; set; }
        public int RatingSnapshot { get; set; }
        public RankedQueueStatus Status { get; set; }
        public Guid? MatchId { get; set; }
        public DateTime EnqueuedAt { get; set; }
    }

    public class MatchHistoryDto
    {
        public Guid MatchId { get; set; }
        public Guid GameId { get; set; }
        public string Mode { get; set; }
        public MatchCompletionStatus Status { get; set; }
        public long? WinnerUserId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }
        public string ResultsJson { get; set; }
        public int ReplayEventCount { get; set; }
        public int ReplayDurationSeconds { get; set; }
    }

    public class RankedStatusDto
    {
        public int Rating { get; set; }
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public RankedQueueDto Queue { get; set; }
        public List<MatchHistoryDto> RecentMatches { get; set; } = new();
    }
}
