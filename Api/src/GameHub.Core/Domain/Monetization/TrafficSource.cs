namespace GameHub.Monetization
{
    /// <summary>
    /// Origin of the player traffic for a play session.
    /// </summary>
    public enum TrafficSource
    {
        /// <summary>Traffic source is unknown or not set.</summary>
        Unknown = 0,

        /// <summary>Player reached the game directly (URL, bookmark).</summary>
        Direct = 1,

        /// <summary>Player came from the GameHub homepage.</summary>
        Homepage = 2,

        /// <summary>Player came from an external search engine.</summary>
        Search = 3,

        /// <summary>Player came from a GameHub internal recommendation or category page.</summary>
        Platform = 4,

        /// <summary>Player came from the Poki network.</summary>
        Poki = 5,

        /// <summary>Player came from a paid or tracked campaign.</summary>
        Campaign = 6
    }
}
