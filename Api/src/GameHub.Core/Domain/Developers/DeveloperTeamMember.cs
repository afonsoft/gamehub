using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Developers
{
    /// <summary>
    /// Membership of a user in a developer team.
    /// </summary>
    public class DeveloperTeamMember : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid TeamId { get; set; }

        public long UserId { get; set; }

        public DeveloperTeamRole Role { get; set; }

        public DateTime InvitedAt { get; set; }

        public DateTime? AcceptedAt { get; set; }

        [StringLength(128)]
        public string InvitationToken { get; set; }

        public virtual DeveloperTeam Team { get; set; }
    }
}
