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

        public Task<LeaderboardEntryDto> GetMyRankAsync(Guid gameId, long userId, CancellationToken cancellationToken = default)
        {
            if (!_scores.TryGetValue(gameId, out var gameScores) || !gameScores.ContainsKey(userId))
            {
                return Task.FromResult<LeaderboardEntryDto>(null);
            }

            var ordered = gameScores.OrderByDescending(x => x.Value).ToList();
            var rank = ordered.FindIndex(x => x.Key == userId) + 1;
            var score = gameScores[userId];

            return Task.FromResult(new LeaderboardEntryDto
            {
                Rank = rank,
                UserId = userId,
                Score = score,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }
}
