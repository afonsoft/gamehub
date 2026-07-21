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
    public class PlaySession : Entity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public PlaySession() { }

        public Guid GameId { get; set; }
        public long? UserId { get; set; }
        [StringLength(128)]
        public string AnonymousIdHash { get; set; }
        [Required]
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        [Required]
        [StringLength(32)]
        public string DeviceType { get; set; }
        [Required]
        [StringLength(64)]
        public string Browser { get; set; }
        [StringLength(2)]
        public string CountryCode { get; set; }
        [StringLength(1024)]
        public string Referrer { get; set; }

        public virtual Game Game { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<GameplayEvent> GameplayEvents { get; protected set; } = new List<GameplayEvent>();
    }
}