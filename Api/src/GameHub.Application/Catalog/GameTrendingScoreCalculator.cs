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

        public async Task<Dictionary<Guid, double>> CalculateGrowthScoresAsync(int days, CancellationToken cancellationToken = default)
        {
            var currentWindowStart = Clock.Now.AddDays(-days).Date;
            var previousWindowStart = Clock.Now.AddDays(-days * 2).Date;

            var current = await _metricSnapshotRepository.GetAll()
                .Where(m => m.Date >= currentWindowStart)
                .GroupBy(m => m.GameId)
                .Select(g => new
                {
                    GameId = g.Key,
                    Score = g.Sum(m => (double?)m.Plays) ?? 0
                })
                .ToDictionaryAsync(x => x.GameId, x => x.Score, cancellationToken);

            var previous = await _metricSnapshotRepository.GetAll()
                .Where(m => m.Date >= previousWindowStart && m.Date < currentWindowStart)
                .GroupBy(m => m.GameId)
                .Select(g => new
                {
                    GameId = g.Key,
                    Score = g.Sum(m => (double?)m.Plays) ?? 0
                })
                .ToDictionaryAsync(x => x.GameId, x => x.Score, cancellationToken);

            var gameIds = current.Keys.Union(previous.Keys);
            var growthScores = new Dictionary<Guid, double>();

            foreach (var gameId in gameIds)
            {
                var currentScore = current.GetValueOrDefault(gameId);
                var previousScore = previous.GetValueOrDefault(gameId);

                growthScores[gameId] = previousScore > 0
                    ? currentScore / previousScore
                    : currentScore;
            }

            return growthScores;
        }
    }
}
