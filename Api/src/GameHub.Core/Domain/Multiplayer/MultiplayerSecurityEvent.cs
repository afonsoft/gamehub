using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Multiplayer
{
    public class MultiplayerSecurityEvent : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public Guid? MatchId { get; set; }
        [Required] public Guid GameId { get; set; }
        public long? UserId { get; set; }
        [StringLength(256)] public string ConnectionId { get; set; }
        [Required, StringLength(64)] public string EventType { get; set; }
        [Required, StringLength(512)] public string Reason { get; set; }
        [StringLength(128)] public string PayloadHash { get; set; }
        [Required] public DateTime OccurredAt { get; set; }
    }
}
