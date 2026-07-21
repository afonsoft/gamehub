using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Configuration
{
    /// <summary>
    /// Feature toggle persistente por tenant.
    /// </summary>
    public class FeatureFlag : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; } = string.Empty;

        [StringLength(512)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public bool IsEnabled { get; set; }
    }
}
