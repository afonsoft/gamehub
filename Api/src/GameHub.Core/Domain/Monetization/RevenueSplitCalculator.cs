namespace GameHub.Monetization
{
    /// <summary>
    /// Calculates revenue split between developer and platform.
    /// </summary>
    public static class RevenueSplitCalculator
    {
        /// <summary>
        /// Returns the developer share (0 to 1) for the given contract and traffic source.
        /// </summary>
        public static decimal GetDeveloperShare(RevenueContractType contractType, TrafficSource trafficSource)
        {
            if (contractType == RevenueContractType.NonExclusive)
            {
                // Non-exclusive games receive a flat fee; no additional revenue share is calculated here.
                return 0m;
            }

            if (contractType == RevenueContractType.WebExclusive)
            {
                // Web exclusive: 100% for direct traffic; 50% when the platform brings the player.
                return trafficSource == TrafficSource.Direct ? 1m : 0.5m;
            }

            return 0.5m;
        }

        /// <summary>
        /// Returns the platform share (0 to 1) for the given contract and traffic source.
        /// </summary>
        public static decimal GetPlatformShare(RevenueContractType contractType, TrafficSource trafficSource)
        {
            return 1m - GetDeveloperShare(contractType, trafficSource);
        }
    }
}
