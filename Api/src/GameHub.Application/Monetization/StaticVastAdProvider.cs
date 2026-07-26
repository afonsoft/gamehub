using System;
using System.Threading;
using System.Threading.Tasks;
using GameHub.Configuration;
using Microsoft.Extensions.Options;

namespace GameHub.Monetization
{
    /// <summary>
    /// Static VAST ad provider example. Simulates a fixed-duration ad break
    /// without external network calls.
    /// </summary>
    public class StaticVastAdProvider : IAdProvider
    {
        private readonly int _defaultDurationSeconds;

        public StaticVastAdProvider(IOptions<StaticVastAdOptions> options)
        {
            _defaultDurationSeconds = options?.Value?.DefaultDurationSeconds ?? 15;
        }

        public Task<AdBreakResult> ShowCommercialBreakAsync(Guid gameId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AdBreakResult
            {
                Completed = true,
                AdBlocked = false,
                AdDurationSeconds = _defaultDurationSeconds,
                Earnings = GameHubConsts.EstimatedCommercialBreakRevenue
            });
        }

        public Task<AdBreakResult> ShowRewardedBreakAsync(Guid gameId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AdBreakResult
            {
                Completed = true,
                RewardGranted = true,
                AdBlocked = false,
                AdDurationSeconds = _defaultDurationSeconds,
                Earnings = GameHubConsts.EstimatedRewardedBreakRevenue
            });
        }
    }
}
