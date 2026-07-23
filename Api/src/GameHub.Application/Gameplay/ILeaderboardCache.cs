using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameHub.Gameplay.Dto;

namespace GameHub.Gameplay
{
    public interface ILeaderboardCache
    {
        Task SubmitScoreAsync(Guid gameId, long userId, long score, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<LeaderboardEntryDto>> GetTopAsync(Guid gameId, int take, CancellationToken cancellationToken = default);

        Task<LeaderboardEntryDto> GetMyRankAsync(Guid gameId, long userId, CancellationToken cancellationToken = default);
    }
}
