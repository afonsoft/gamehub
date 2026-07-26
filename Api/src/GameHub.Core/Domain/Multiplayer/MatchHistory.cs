using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Multiplayer
{
    public enum MatchCompletionStatus
    {
        Completed = 0,
        Abandoned = 1,
        Cancelled = 2
    }

    public class MatchHistory : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [Required] public Guid MatchId { get; set; }
        [Required] public Guid GameId { get; set; }
        public Guid? SeasonId { get; set; }
        [StringLength(64)] public string Mode { get; set; }
        [Required] public MatchCompletionStatus Status { get; set; }
        public long? WinnerUserId { get; set; }
        [Required] public DateTime StartedAt { get; set; }
        [Required] public DateTime EndedAt { get; set; }
        [StringLength(16000)] public string ResultsJson { get; set; }
        public virtual ReplayMetadata Replay { get; set; }
    }

    public class ReplayMetadata : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [Required] public Guid MatchHistoryId { get; set; }
        [StringLength(512)] public string StorageKey { get; set; }
        public int EventCount { get; set; }
        public int DurationSeconds { get; set; }
        [StringLength(128)] public string ContentHash { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
