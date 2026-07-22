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
        /// <summary>Exibe um break comercial e aguarda a conclusão.</summary>
        Task ShowCommercialBreakAsync(Guid gameId, CancellationToken cancellationToken = default);

        /// <summary>Exibe um rewarded ad e retorna true se o usuário completou.</summary>
        Task<bool> ShowRewardedBreakAsync(Guid gameId, CancellationToken cancellationToken = default);
    }
}
