using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities.Auditing;

namespace GameHub.MultiTenancy
{
    /// <summary>
    /// Association between a host-level user and one of the tenants they can access.
    /// The <see cref="TenantUserId"/> holds the real Abp User Id inside the tenant,
    /// enabling per-tenant permissions and data filtering.
    /// </summary>
    public class UserTenantMembership : CreationAuditedEntity<long>
    {
        [Required]
        public virtual long UserId { get; set; }

        [Required]
        public virtual int TenantId { get; set; }

        /// <summary>
        /// The Id of the shadow <see cref="Eaf.Middleware.Authorization.Users.User"/>
        /// created inside <see cref="TenantId"/> for this membership.
        /// </summary>
        [Required]
        public virtual long TenantUserId { get; set; }

        public virtual bool IsDefault { get; set; }
    }
}
