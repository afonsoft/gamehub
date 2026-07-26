using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Multiplayer
{
    public enum RankedQueueStatus
    {
        Waiting = 0,
        Matched = 1,
        Cancelled = 2,
        Abandoned = 3
    }

    public class RankedQueueEntry : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [Required] public Guid GameId { get; set; }
        [Required] public Guid SeasonId { get; set; }
        [Required] public long UserId { get; set; }
        [Required, StringLength(64)] public string Mode { get; set; }
        [StringLength(64)] public string Region { get; set; }
        public int RatingSnapshot { get; set; }
        [Required] public DateTime EnqueuedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public RankedQueueStatus Status { get; set; }
        public Guid? MatchId { get; set; }
    }
}
