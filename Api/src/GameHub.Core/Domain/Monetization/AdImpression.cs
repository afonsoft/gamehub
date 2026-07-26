using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using GameHub.Catalog;

namespace GameHub.Monetization
{
    /// <summary>
    /// Record of an ad impression served for a game.
    /// </summary>
    public class AdImpression : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        public Guid? BuildId { get; set; }

        [Required]
        [StringLength(32)]
        public string Type { get; set; }

        [Required]
        [StringLength(64)]
        public string Provider { get; set; }

        [StringLength(2)]
        public string CountryCode { get; set; }

        [StringLength(64)]
        public string DeviceType { get; set; }

        /// <summary>Effective CPM in USD cents or full dollars depending on reporting convention.</summary>
        public decimal Cpm { get; set; }

        /// <summary>Estimated earnings attributed to this impression.</summary>
        public decimal Earnings { get; set; }

        [Required]
        public DateTime OccurredAt { get; set; }

        public virtual Game Game { get; set; }
    }
}
