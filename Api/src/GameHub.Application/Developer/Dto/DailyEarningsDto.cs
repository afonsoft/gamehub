using System;

namespace GameHub.Developer.Dto
{
    public class DailyEarningsDto
    {
        public DateTime Date { get; set; }

        public long CommercialBreaks { get; set; }

        public long RewardedBreaks { get; set; }

        public decimal GrossEstimatedRevenue { get; set; }

        public decimal DeveloperEstimatedRevenue { get; set; }
    }
}
