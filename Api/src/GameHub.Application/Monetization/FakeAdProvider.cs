using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameHub.Monetization
{
    /// <summary>
    /// Provedor de anúncios fake/null object para desenvolvimento e MVP.
    /// Não realiza chamadas externas nem acopla o domínio a fornecedores reais.
    /// </summary>
    public class FakeAdProvider : IAdProvider
    {
        private const int CommercialBreakDelayMilliseconds = 1000;

        public async Task ShowCommercialBreakAsync(Guid gameId, CancellationToken cancellationToken = default)
        {
            await Task.Delay(CommercialBreakDelayMilliseconds, cancellationToken);
        }

        public Task<bool> ShowRewardedBreakAsync(Guid gameId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}
