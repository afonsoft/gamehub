using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Gameplay.Dto;

namespace GameHub.Gameplay
{
    public interface ILeaderboardAppService : IApplicationService
    {
        Task SubmitScoreAsync(SubmitScoreInput input);

        Task<ListResultDto<LeaderboardEntryDto>> GetTopAsync(GetLeaderboardInput input);

        Task<LeaderboardEntryDto> GetMyRankAsync(GetLeaderboardInput input);
    }
}
