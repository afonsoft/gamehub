using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Multiplayer.Dto;

namespace GameHub.Multiplayer
{
    /// <summary>
    /// Application service for multiplayer match management.
    /// </summary>
    public interface IMultiplayerAppService : IApplicationService
    {
        Task<MatchDto> CreateMatchAsync(CreateMatchInput input);

        Task<MatchDto> CreateOrJoinMatchAsync(CreateMatchInput input);

        Task<MatchDto> JoinMatchAsync(JoinMatchInput input);

        Task<MatchDto> JoinMatchByRoomCodeAsync(JoinMatchByRoomCodeInput input);

        Task<MatchDto> SpectateMatchAsync(Guid matchId, string anonymousIdHash = null, string connectionId = null);

        Task LeaveMatchAsync(LeaveMatchInput input);

        Task<MatchDto> GetMatchAsync(Guid matchId);

        Task UpdateMatchStateAsync(UpdateMatchStateInput input);

        Task EndMatchAsync(Guid matchId);

        Task<List<MatchBrowserDto>> BrowseMatchesAsync(BrowseMatchesInput input);

        Task<RankedQueueDto> EnqueueRankedAsync(EnqueueRankedInput input);

        Task CancelRankedAsync(CancelRankedInput input);

        Task<RankedStatusDto> GetRankedStatusAsync(Guid gameId, string mode);

        Task<List<MatchHistoryDto>> GetMatchHistoryAsync(Guid gameId, int maxResultCount = 20);

        Task CompleteMatchAsync(CompleteMatchInput input);
    }
}
