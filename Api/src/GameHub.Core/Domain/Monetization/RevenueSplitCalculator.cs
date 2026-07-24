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
            if (trafficSource == TrafficSource.Direct)
            {
                return 1m;
            }

            if (contractType == RevenueContractType.WebExclusive)
            {
                return 0.7m;
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
