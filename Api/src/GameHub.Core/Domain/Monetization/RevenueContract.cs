using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using GameHub.Catalog;

namespace GameHub.Monetization
{
    /// <summary>
    /// Revenue-sharing contract for a game.
    /// </summary>
    public class RevenueContract : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        public RevenueContract() { }

        public RevenueContract(Guid id, Guid gameId, RevenueContractType contractType)
        {
            Id = id;
            GameId = gameId;
            ContractType = contractType;
            EffectiveDate = DateTime.UtcNow;
            IsActive = true;
        }

        [Required]
        public Guid GameId { get; set; }

        [Required]
        public RevenueContractType ContractType { get; set; }

        public DateTime EffectiveDate { get; set; }

        public bool IsActive { get; set; }

        public virtual Game Game { get; set; }
    }
}
