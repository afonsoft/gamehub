using System;

namespace GameHub.Jobs
{
    /// <summary>
    /// Arguments for the daily gameplay metrics aggregation job.
    /// </summary>
    public class GameMetricsAggregationArgs
    {
        /// <summary>
        /// Date (UTC) for which metrics should be aggregated.
        /// </summary>
        public DateTime Date { get; set; }
    }
}
