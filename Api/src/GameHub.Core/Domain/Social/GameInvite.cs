using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Social
{
    /// <summary>Tenant-aware invitation to join a game match.</summary>
    public class GameInvite : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        [Required]
        public Guid MatchId { get; set; }

        [Required]
        public long InviterUserId { get; set; }

        [Required]
        public long InviteeUserId { get; set; }

        [Required]
        [StringLength(32)]
        public string Status { get; set; } = "pending";

        public DateTime ExpiresAt { get; set; }

        public DateTime? AcceptedAt { get; set; }
    }
}
