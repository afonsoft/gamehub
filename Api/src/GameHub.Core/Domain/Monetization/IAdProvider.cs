using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameHub.Monetization
{
    /// <summary>
    /// Abstração de provedor de anúncios do GameHub.
    /// O domínio não conhece implementações concretas (provider fake/null object).
    /// </summary>
    public interface IAdProvider
    {
        /// <summary>Shows a commercial break and returns the interaction result.</summary>
        Task<AdBreakResult> ShowCommercialBreakAsync(Guid gameId, CancellationToken cancellationToken = default);

        /// <summary>Shows a rewarded ad and returns the interaction result.</summary>
        Task<AdBreakResult> ShowRewardedBreakAsync(Guid gameId, CancellationToken cancellationToken = default);
    }
}
