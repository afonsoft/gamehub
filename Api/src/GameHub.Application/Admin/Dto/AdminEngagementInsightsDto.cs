using System;
using System.Collections.Generic;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Engagement insights based on session duration and benchmarks by category.
    /// </summary>
    public class AdminEngagementInsightsDto
    {
        public Guid GameId { get; set; }
        public string GameTitle { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double AverageSessionDurationSeconds { get; set; }
        public double MedianSessionDurationSeconds { get; set; }
        public double BenchmarkSeconds { get; set; }
        public bool BelowBenchmark { get; set; }
        public List<string> Suggestions { get; set; } = new();
    }
}
