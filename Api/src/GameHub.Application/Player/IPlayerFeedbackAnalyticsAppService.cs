using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Player.Dto;

namespace GameHub.Player
{
    public interface IPlayerFeedbackAnalyticsAppService : IApplicationService
    {
        Task<PlayerFeedbackSummaryDto> GetFeedbackSummaryAsync(Guid gameId);
    }
}
