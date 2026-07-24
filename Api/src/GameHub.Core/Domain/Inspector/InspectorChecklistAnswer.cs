using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.MultiTenancy;

namespace GameHub.Inspector
{
    /// <summary>
    /// QA checklist answer recorded during an inspector session.
    /// </summary>
    public class InspectorChecklistAnswer : Entity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid SessionId { get; set; }

        [Required]
        [StringLength(64)]
        public string QuestionId { get; set; }

        [StringLength(2000)]
        public string Answer { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual InspectorSession Session { get; set; }
    }
}
