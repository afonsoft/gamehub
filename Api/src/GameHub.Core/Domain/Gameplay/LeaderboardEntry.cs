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

namespace GameHub.Gameplay
{
    public class LeaderboardEntry : Entity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public LeaderboardEntry() { }

        public Guid GameId { get; set; }
        public long UserId { get; set; }
        [Required]
        public long Score { get; set; }
        [StringLength(4000)]
        public string MetadataJson { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime UpdatedAt { get; set; }

        public virtual Game Game { get; set; }
        public virtual User User { get; set; }
    }
}