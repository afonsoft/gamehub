using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Multiplayer.Dto
{
    public class BrowseMatchesInput
    {
        public Guid? GameId { get; set; }
        public string Mode { get; set; }
        public string Region { get; set; }
        public int? MaxLatencyMs { get; set; }
        public bool? IsRanked { get; set; }
        [Range(1, 100)] public int MaxResultCount { get; set; } = 25;
        [Range(0, int.MaxValue)] public int SkipCount { get; set; }
    }

    public class EnqueueRankedInput
    {
        [Required] public Guid GameId { get; set; }
        [Required, StringLength(64)] public string Mode { get; set; }
        [StringLength(64)] public string Region { get; set; }
    }

    public class CancelRankedInput
    {
        [Required] public Guid QueueEntryId { get; set; }
    }

    public class CompleteMatchInput
    {
        [Required] public Guid MatchId { get; set; }
        public long? WinnerUserId { get; set; }
        public MatchCompletionStatus Status { get; set; }
        [StringLength(16000)] public string ResultsJson { get; set; }
        public int ReplayEventCount { get; set; }
        public int ReplayDurationSeconds { get; set; }
    }
}
