using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Developers
{
    /// <summary>
    /// A developer team (P4D team settings) that can own games and billing information.
    /// </summary>
    public class DeveloperTeam : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [Required]
        [StringLength(256)]
        public string PrimaryContactEmail { get; set; }

        [StringLength(128)]
        public string Country { get; set; }

        [StringLength(128)]
        public string ApiKey { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<DeveloperTeamMember> Members { get; protected set; } = new List<DeveloperTeamMember>();
    }
}
