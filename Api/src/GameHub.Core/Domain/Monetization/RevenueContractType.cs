namespace GameHub.Monetization
{
    /// <summary>
    /// Type of revenue-sharing contract for a game.
    /// </summary>
    public enum RevenueContractType
    {
        /// <summary>The game is exclusive to the GameHub web platform.</summary>
        WebExclusive = 0,

        /// <summary>The game is non-exclusive and may be distributed elsewhere.</summary>
        NonExclusive = 1
    }
}
