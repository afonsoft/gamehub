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

namespace GameHub.Gameplay
{
    public class GameplayEvent : Entity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public GameplayEvent() { }

        public Guid PlaySessionId { get; set; }
        public Guid GameId { get; set; }
        public Guid? BuildId { get; set; }
        public Guid? MatchId { get; set; }
        [Required]
        public GameplayEventType EventType { get; set; }
        [StringLength(128)]
        public string EventName { get; set; }
        [StringLength(4000)]
        public string PayloadJson { get; set; }
        [Required]
        public DateTime OccurredAt { get; set; }

        public virtual PlaySession PlaySession { get; set; }
        public virtual Game Game { get; set; }
    }
}