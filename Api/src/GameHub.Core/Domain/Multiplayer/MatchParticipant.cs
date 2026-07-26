using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Multiplayer
{
    /// <summary>
    /// A participant in a lightweight online match.
    /// </summary>
    public class MatchParticipant : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid MatchId { get; set; }

        public long? UserId { get; set; }

        [StringLength(128)]
        public string AnonymousIdHash { get; set; }

        [StringLength(256)]
        public string ConnectionId { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public bool IsSpectator { get; set; }

        public DateTime JoinedAt { get; set; }

        public DateTime? LeftAt { get; set; }

        public DateTime? DisconnectedAt { get; set; }

        public DateTime? GracePeriodEndsAt { get; set; }

        public virtual MatchState Match { get; set; }
    }
}
