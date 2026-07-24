using System;
using System.Threading;
using System.Threading.Tasks;
using GameHub.Configuration;
using Microsoft.Extensions.Options;

namespace GameHub.Monetization
{
    /// <summary>
    /// Routes ad break calls to the configured provider implementation.
    /// </summary>
    public class ConfigurableAdProvider : IAdProvider
    {
        private readonly IAdProvider _inner;

        public ConfigurableAdProvider(IOptions<AdBreakOptions> adBreakOptions, IOptions<StaticVastAdOptions> staticVastOptions)
        {
            var options = adBreakOptions?.Value ?? new AdBreakOptions();
            var provider = options.Provider?.Trim() ?? "Fake";

            if (provider.Equals("StaticVast", StringComparison.OrdinalIgnoreCase))
            {
                _inner = new StaticVastAdProvider(staticVastOptions);
            }
            else
            {
                _inner = new FakeAdProvider();
            }
        }

        public Task<AdBreakResult> ShowCommercialBreakAsync(Guid gameId, CancellationToken cancellationToken = default)
        {
            return _inner.ShowCommercialBreakAsync(gameId, cancellationToken);
        }

        public Task<AdBreakResult> ShowRewardedBreakAsync(Guid gameId, CancellationToken cancellationToken = default)
        {
            return _inner.ShowRewardedBreakAsync(gameId, cancellationToken);
        }
    }
}
