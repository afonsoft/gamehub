using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Runtime.Session;
using GameHub.Gameplay;
using GameHub.Gameplay.Dto;
using StackExchange.Redis;

namespace GameHub.Web.Caching
{
    /// <summary>
    /// Redis-backed implementation of <see cref="ILeaderboardCache"/> using sorted sets.
    /// Cache key is scoped per tenant to support GameHub multi-tenancy.
    /// </summary>
    public class RedisLeaderboardCache : ILeaderboardCache
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IAbpSession _abpSession;

        public RedisLeaderboardCache(IConnectionMultiplexer redis, IAbpSession abpSession)
        {
            _redis = redis;
            _abpSession = abpSession;
        }

        public async Task SubmitScoreAsync(Guid gameId, long userId, long score, CancellationToken cancellationToken = default)
        {
            var db = _redis.GetDatabase();
            await db.SortedSetIncrementAsync(GetKey(gameId), userId.ToString(), score);
        }

        public async Task<IReadOnlyList<LeaderboardEntryDto>> GetTopAsync(Guid gameId, int take, CancellationToken cancellationToken = default)
        {
            var db = _redis.GetDatabase();
            var entries = await db.SortedSetRangeByRankWithScoresAsync(
                GetKey(gameId),
                start: 0,
                stop: take - 1,
                order: Order.Descending);

            var rank = 1;
            return entries
                .Select(e => new LeaderboardEntryDto
                {
                    Rank = rank++,
                    UserId = long.Parse(e.Element),
                    Score = (long)e.Score,
                    DisplayName = string.Empty,
                    UpdatedAt = DateTime.UtcNow
                })
                .ToList();
        }

        private string GetKey(Guid gameId)
        {
            var tenantId = _abpSession.TenantId?.ToString() ?? "host";
            return $"leaderboard:{tenantId}:{gameId:N}";
        }
    }
}
