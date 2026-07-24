using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.MultiTenancy;

namespace GameHub.Builds
{
    /// <summary>
    /// Short-lived preview token that grants access to a specific game build version.
    /// </summary>
    public class PreviewToken : Entity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        [Required]
        public Guid GameBuildId { get; set; }

        [Required]
        [StringLength(64)]
        public string Version { get; set; }

        [Required]
        [StringLength(4096)]
        public string TokenValue { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        public long? CreatedByUserId { get; set; }

        public virtual GameBuild GameBuild { get; set; }
    }
}
