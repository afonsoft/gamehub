using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Inspector
{
    /// <summary>
    /// A QA/inspector session for running a game build in a controlled environment.
    /// </summary>
    public class InspectorSession : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        public Guid? GameBuildId { get; set; }

        [Required]
        public DateTime StartedAt { get; set; }

        [StringLength(32)]
        public string DevicePreset { get; set; }

        [StringLength(32)]
        public string Resolution { get; set; }

        [StringLength(32)]
        public string Status { get; set; } = "Running";

        public virtual GameHub.Catalog.Game Game { get; set; }
    }
}
