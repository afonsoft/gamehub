using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Monetization.Dto;

namespace GameHub.Monetization
{
    /// <summary>
    /// Contrato do serviço de breaks de anúncios.
    /// </summary>
    public interface IAdBreakAppService : IApplicationService
    {
        /// <summary>Solicita um break comercial.</summary>
        Task<CommercialBreakResultDto> RequestCommercialBreakAsync(RequestAdBreakInput input);

        /// <summary>Solicita um rewarded ad break.</summary>
        Task<RewardedBreakResultDto> RequestRewardedBreakAsync(RequestAdBreakInput input);
    }
}
