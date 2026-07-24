using GameHub.Monetization;

namespace GameHub.Monetization.Dto
{
    /// <summary>
    /// Result of a revenue split calculation.
    /// </summary>
    public class RevenueShareResultDto
    {
        public TrafficSource TrafficSource { get; set; }

        public RevenueContractType ContractType { get; set; }

        /// <summary>Developer share (0 to 1).</summary>
        public decimal DeveloperShare { get; set; }

        /// <summary>Platform share (0 to 1).</summary>
        public decimal PlatformShare { get; set; }
    }
}
