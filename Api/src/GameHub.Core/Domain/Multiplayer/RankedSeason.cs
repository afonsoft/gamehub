using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Multiplayer
{
    public class RankedSeason : FullAuditedAggregateRoot<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [Required] public Guid GameId { get; set; }
        [Required, StringLength(64)] public string Mode { get; set; }
        [Required, StringLength(128)] public string Name { get; set; }
        [Required] public DateTime StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public bool IsActive { get; set; }
    }
}
