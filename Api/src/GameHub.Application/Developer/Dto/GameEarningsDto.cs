using System;
using System.Collections.Generic;
using GameHub.Monetization;

namespace GameHub.Developer.Dto
{
    public class GameEarningsDto
    {
        public Guid GameId { get; set; }

        public string GameTitle { get; set; } = string.Empty;

        public long TotalPlays { get; set; }

        public long CommercialBreaks { get; set; }

        public long RewardedBreaks { get; set; }

        public decimal GrossEstimatedRevenue { get; set; }

        public decimal DeveloperEstimatedRevenue { get; set; }

        public decimal PlatformEstimatedRevenue { get; set; }

        public decimal FlatFeeAmount { get; set; }

        public decimal DeveloperShare { get; set; }

        public RevenueContractType ContractType { get; set; }

        public List<DailyEarningsDto> Daily { get; set; } = new();
    }
}
