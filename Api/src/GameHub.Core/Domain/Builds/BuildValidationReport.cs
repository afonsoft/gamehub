using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Builds
{
    /// <summary>
    /// Persisted result of a build package validation run.
    /// </summary>
    public class BuildValidationReport : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid GameBuildId { get; set; }

        public bool IsValid { get; set; }

        [StringLength(4000)]
        public string ErrorsJson { get; set; }

        [StringLength(4000)]
        public string WarningsJson { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual GameBuild GameBuild { get; set; }
    }
}
