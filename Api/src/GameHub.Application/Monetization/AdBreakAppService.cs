using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Monetization.Dto;

namespace GameHub.Monetization
{
    /// <summary>
    /// Serviço de monetização: orquestra breaks comerciais e rewarded ads via IAdProvider.
    /// </summary>
    public class AdBreakAppService : GameHubAppServiceBase, IAdBreakAppService
    {
        private readonly IAdProvider _adProvider;

        public AdBreakAppService(IAdProvider adProvider)
        {
            _adProvider = adProvider;
        }

        public async Task<CommercialBreakResultDto> RequestCommercialBreakAsync(RequestAdBreakInput input)
        {
            await _adProvider.ShowCommercialBreakAsync(input.GameId);

            return new CommercialBreakResultDto
            {
                Completed = true,
                DurationSeconds = 1
            };
        }

        public async Task<RewardedBreakResultDto> RequestRewardedBreakAsync(RequestAdBreakInput input)
        {
            var completed = await _adProvider.ShowRewardedBreakAsync(input.GameId);

            return new RewardedBreakResultDto
            {
                Completed = completed
            };
        }
    }
}
