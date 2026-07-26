using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Gameplay.Dto;
using GameHub.Multiplayer.Dto;
using GameHub.ArbitraryUserData.Dto;

namespace GameHub.Gameplay
{
    public interface IGameplayAppService : IApplicationService
    {
        Task<PlaySessionDto> StartSessionAsync(StartPlaySessionInput input);

        Task<PlaySessionDto> StopSessionAsync(Guid sessionId);

        Task EventAsync(GameplayEventInput input);

        Task UpdateFpsAsync(UpdateFpsInput input);

        Task<GameErrorLogDto> CaptureErrorAsync(CaptureGameErrorInput input);

        Task<MatchDto> CreateMatchAsync(CreateMatchInput input);

        Task<MatchDto> JoinMatchAsync(JoinMatchInput input);

        Task<MatchDto> JoinMatchByRoomCodeAsync(JoinMatchByRoomCodeInput input);

        Task LeaveMatchAsync(LeaveMatchInput input);

        Task SendMatchStateAsync(UpdateMatchStateInput input);

        Task<string> LoadArbitraryAsync(GetArbitraryUserDataInput input);

        Task SaveArbitraryAsync(SetArbitraryUserDataInput input);

        Task DeleteArbitraryAsync(DeleteArbitraryUserDataInput input);
    }
}
