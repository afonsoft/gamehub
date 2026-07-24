using GameHub.Moderation;
using GameHub.Gameplay;
using GameHub.Developers;
using GameHub.Catalog;
using GameHub.Builds;
using GameHub;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Catalog
{
    public class Category : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public Category() { }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }
        [Required]
        [StringLength(128)]
        public string Slug { get; set; }
        [Required]
        public int SortOrder { get; set; }
        [Required]
        public bool IsActive { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(256)]
        public string Keywords { get; set; }

        public virtual ICollection<GameCategory> GameCategories { get; protected set; } = new List<GameCategory>();
    }
}