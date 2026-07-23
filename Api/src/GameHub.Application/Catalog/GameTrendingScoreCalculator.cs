using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Timing;
using GameHub.Catalog;
using GameHub.Gameplay;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Catalog
{
    /// <summary>
    /// Calculates trending scores from recent <see cref="GameMetricSnapshot"/> data.
    /// </summary>
    public class GameTrendingScoreCalculator : ITrendingScoreCalculator, ITransientDependency
    {
        private readonly IRepository<GameMetricSnapshot, Guid> _metricSnapshotRepository;

        public GameTrendingScoreCalculator(IRepository<GameMetricSnapshot, Guid> metricSnapshotRepository)
        {
            _metricSnapshotRepository = metricSnapshotRepository;
        }

        public async Task<Dictionary<Guid, double>> CalculateScoresAsync(int days, CancellationToken cancellationToken = default)
        {
            var fromDate = Clock.Now.AddDays(-days).Date;

            var scores = await _metricSnapshotRepository.GetAll()
                .Where(m => m.Date >= fromDate)
                .GroupBy(m => m.GameId)
                .Select(g => new
                {
                    GameId = g.Key,
                    Score = g.Sum(m => (double?)m.Plays) ?? 0
                })
                .ToDictionaryAsync(x => x.GameId, x => x.Score, cancellationToken);

            return scores;
        }
    }
}
