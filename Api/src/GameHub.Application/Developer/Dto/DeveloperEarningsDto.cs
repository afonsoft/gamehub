using System;
using System.Collections.Generic;

namespace GameHub.Developer.Dto
{
    public class DeveloperEarningsDto
    {
        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public decimal TotalGrossEstimatedRevenue { get; set; }

        public decimal TotalDeveloperEstimatedRevenue { get; set; }

        public decimal TotalPlatformEstimatedRevenue { get; set; }

        public long TotalCommercialBreaks { get; set; }

        public long TotalRewardedBreaks { get; set; }

        public List<GameEarningsDto> Games { get; set; } = new();
    }
}
