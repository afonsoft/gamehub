using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GameHub.Catalog
{
    /// <summary>
    /// Calculates a trending score for each published game based on recent metric snapshots.
    /// </summary>
    public interface ITrendingScoreCalculator
    {
        /// <summary>
        /// Returns a dictionary of game IDs to their total play count for the last <paramref name="days"/> days.
        /// </summary>
        /// <param name="days">Number of days to look back.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Game ID to score mapping.</returns>
        Task<Dictionary<Guid, double>> CalculateScoresAsync(int days, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a dictionary of game IDs to their growth ratio between the last <paramref name="days"/> days and the previous period.
        /// </summary>
        /// <param name="days">Number of days per window.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Game ID to growth score mapping.</returns>
        Task<Dictionary<Guid, double>> CalculateGrowthScoresAsync(int days, CancellationToken cancellationToken = default);
    }
}
