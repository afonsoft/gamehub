using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameHub.Gameplay.Dto;

namespace GameHub.Gameplay
{
    public class InMemoryLeaderboardCache : ILeaderboardCache
    {
        // gameId -> (userId -> score)
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<long, long>> _scores = new ConcurrentDictionary<Guid, ConcurrentDictionary<long, long>>();

        public Task SubmitScoreAsync(Guid gameId, long userId, long score, CancellationToken cancellationToken = default)
        {
            var gameScores = _scores.GetOrAdd(gameId, _ => new ConcurrentDictionary<long, long>());
            gameScores.AddOrUpdate(userId, score, (_, existing) => Math.Max(existing, score));
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<LeaderboardEntryDto>> GetTopAsync(Guid gameId, int take, CancellationToken cancellationToken = default)
        {
            if (!_scores.TryGetValue(gameId, out var gameScores))
            {
                return Task.FromResult<IReadOnlyList<LeaderboardEntryDto>>(new List<LeaderboardEntryDto>());
            }

            var rank = 1;
            var entries = gameScores
                .OrderByDescending(x => x.Value)
                .Take(take)
                .Select(x => new LeaderboardEntryDto
                {
                    Rank = rank++,
                    UserId = x.Key,
                    Score = x.Value,
                    UpdatedAt = DateTime.UtcNow
                })
                .ToList();

            return Task.FromResult<IReadOnlyList<LeaderboardEntryDto>>(entries);
        }
    }
}
