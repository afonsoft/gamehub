using System;
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

        Task LeaveMatchAsync(LeaveMatchInput input);

        Task<MatchDto> GetMatchAsync(Guid matchId);

        Task UpdateMatchStateAsync(UpdateMatchStateInput input);

        Task EndMatchAsync(Guid matchId);
    }
}
