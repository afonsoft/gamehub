using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Gameplay.Dto;

namespace GameHub.Gameplay
{
    public interface IGameplayAppService : IApplicationService
    {
        Task<PlaySessionDto> StartSessionAsync(StartPlaySessionInput input);

        Task<PlaySessionDto> StopSessionAsync(Guid sessionId);

        Task EventAsync(GameplayEventInput input);
    }
}
