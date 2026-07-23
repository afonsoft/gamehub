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
        /// Returns a dictionary of game IDs to their trending score for the last <paramref name="days"/> days.
        /// </summary>
        /// <param name="days">Number of days to look back.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Game ID to score mapping.</returns>
        Task<Dictionary<Guid, double>> CalculateScoresAsync(int days, CancellationToken cancellationToken = default);
    }
}
