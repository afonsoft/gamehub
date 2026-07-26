using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Social
{
    /// <summary>Tenant-aware notification delivered to a player.</summary>
    public class GameNotification : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        [StringLength(64)]
        public string NotificationType { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string PayloadJson { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }
    }
}
