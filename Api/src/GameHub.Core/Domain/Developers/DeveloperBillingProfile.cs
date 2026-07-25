using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Developers
{
    /// <summary>
    /// Placeholder billing profile for a developer team.
    /// No real payment data or sensitive information is stored.
    /// </summary>
    public class DeveloperBillingProfile : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid TeamId { get; set; }

        [StringLength(64)]
        public string TaxId { get; set; }

        [StringLength(512)]
        public string Address { get; set; }

        [StringLength(64)]
        public string PaymentMethodPlaceholder { get; set; }

        public bool IsApproved { get; set; }

        public bool IsPendingReview { get; set; }

        public virtual DeveloperTeam Team { get; set; }
    }
}
