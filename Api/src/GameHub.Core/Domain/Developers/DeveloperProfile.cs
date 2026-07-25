using GameHub.Moderation;
using GameHub.Gameplay;
using GameHub.Developers;
using GameHub.Catalog;
using GameHub.Builds;
using GameHub;
using Eaf.Middleware.Authorization.Users;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Developers
{
    public class DeveloperProfile : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public DeveloperProfile() { }

        public long UserId { get; set; }
        [Required]
        [StringLength(128)]
        public string DisplayName { get; set; }
        [StringLength(256)]
        public string LegalName { get; set; }
        [StringLength(512)]
        public string WebsiteUrl { get; set; }
        [StringLength(256)]
        public string SupportEmail { get; set; }
        [StringLength(128)]
        public string ApiKey { get; set; }
        [Required]
        public DeveloperProfileStatus Status { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Game> Games { get; protected set; } = new List<Game>();
    }
}