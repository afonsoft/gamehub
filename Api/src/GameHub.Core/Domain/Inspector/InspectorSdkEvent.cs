using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.MultiTenancy;

namespace GameHub.Inspector
{
    /// <summary>
    /// SDK event recorded during an inspector session.
    /// </summary>
    public class InspectorSdkEvent : Entity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid SessionId { get; set; }

        [Required]
        [StringLength(64)]
        public string EventType { get; set; }

        [StringLength(2000)]
        public string Payload { get; set; }

        public long SequenceNumber { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        public virtual InspectorSession Session { get; set; }
    }
}
