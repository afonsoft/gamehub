using System;
using GameHub.Monetization;

namespace GameHub.Monetization.Dto
{
    /// <summary>
    /// DTO for a revenue contract.
    /// </summary>
    public class RevenueContractDto
    {
        public Guid Id { get; set; }

        public Guid GameId { get; set; }

        public RevenueContractType ContractType { get; set; }

        public DateTime EffectiveDate { get; set; }

        public bool IsActive { get; set; }
    }
}
