using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.MultiTenancy;

namespace GameHub.Inspector
{
    /// <summary>
    /// Warning detected during an inspector session.
    /// </summary>
    public class InspectorWarning : Entity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid SessionId { get; set; }

        [Required]
        [StringLength(64)]
        public string Category { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        [Required]
        [StringLength(32)]
        public string Severity { get; set; } = "Warning";

        public virtual InspectorSession Session { get; set; }
    }
}
