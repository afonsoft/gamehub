using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities.Auditing;

namespace GameHub.MultiTenancy
{
    /// <summary>
    /// Public request sent by a host user to join an existing tenant.
    /// Must be approved by a tenant administrator before the shadow user is created.
    /// </summary>
    public class TenantJoinRequest : AuditedEntity<long>
    {
        [Required]
        public virtual long UserId { get; set; }

        [Required]
        public virtual int TenantId { get; set; }

        /// <summary>
        /// Pending, Approved or Rejected.
        /// </summary>
        [Required]
        [StringLength(20)]
        public virtual string Status { get; set; } = TenantJoinRequestStatus.Pending;

        /// <summary>
        /// Optional message provided by the requester.
        /// </summary>
        [StringLength(1024)]
        public virtual string Message { get; set; }
    }

    public static class TenantJoinRequestStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
    }
}
