using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using GameHub.Catalog;

namespace GameHub.Builds
{
    /// <summary>
    /// Request to allow an external domain (e.g. analytics provider) to be used by a game build.
    /// </summary>
    public class ExternalResourceExemption : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        [Required]
        [StringLength(256)]
        public string Domain { get; set; }

        [StringLength(128)]
        public string ProviderName { get; set; }

        [StringLength(512)]
        public string PrivacyStatementUrl { get; set; }

        [Required]
        public ExternalResourceExemptionStatus Status { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RejectedAt { get; set; }

        [StringLength(1000)]
        public string ModeratorNotes { get; set; }

        public virtual Game Game { get; set; }
    }

    public enum ExternalResourceExemptionStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}
