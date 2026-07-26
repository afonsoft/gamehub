using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameHub.Monetization
{
    /// <summary>
    /// Fake/null object ad provider for development and MVP.
    /// Simulates success, ad-blocked and rewarded scenarios without external calls.
    /// </summary>
    public class FakeAdProvider : IAdProvider
    {
        private const int CommercialBreakDelayMilliseconds = 1000;

        /// <summary>When true, simulates an ad-blocked environment.</summary>
        public bool SimulateAdBlocked { get; set; }

        /// <summary>Overrides the default simulated ad duration in seconds.</summary>
        public int? FixedDurationSeconds { get; set; }

        public async Task<AdBreakResult> ShowCommercialBreakAsync(Guid gameId, CancellationToken cancellationToken = default)
        {
            await Task.Delay(CommercialBreakDelayMilliseconds, cancellationToken);

            if (SimulateAdBlocked)
            {
                return new AdBreakResult
                {
                    Completed = false,
                    AdBlocked = true,
                    AdDurationSeconds = 0,
                    ErrorMessage = "Ad blocked or unavailable."
                };
            }

            return new AdBreakResult
            {
                Completed = true,
                AdBlocked = false,
                AdDurationSeconds = FixedDurationSeconds ?? 1,
                Earnings = GameHubConsts.EstimatedCommercialBreakRevenue
            };
        }

        public async Task<AdBreakResult> ShowRewardedBreakAsync(Guid gameId, CancellationToken cancellationToken = default)
        {
            await Task.Delay(CommercialBreakDelayMilliseconds, cancellationToken);

            if (SimulateAdBlocked)
            {
                return new AdBreakResult
                {
                    Completed = false,
                    RewardGranted = false,
                    AdBlocked = true,
                    AdDurationSeconds = 0,
                    ErrorMessage = "Ad blocked or unavailable."
                };
            }

            return new AdBreakResult
            {
                Completed = true,
                RewardGranted = true,
                AdBlocked = false,
                AdDurationSeconds = FixedDurationSeconds ?? 1,
                Earnings = GameHubConsts.EstimatedRewardedBreakRevenue
            };
        }
    }
}
