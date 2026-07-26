using System;
using System.Collections.Generic;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Insights about where players drop off during the first minute.
    /// </summary>
    public class AdminOnboardingInsightsDto
    {
        public Guid GameId { get; set; }
        public string GameTitle { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double OverallDropOffRate { get; set; }
        public List<MetricDistributionItemDto> DropOffByDevice { get; set; } = new();
        public List<MetricDistributionItemDto> DropOffByCountry { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
    }
}
