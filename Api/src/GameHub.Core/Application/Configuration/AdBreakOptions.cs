namespace GameHub.Configuration
{
    /// <summary>
    /// Configuração global de ad breaks.
    /// </summary>
    public class AdBreakOptions
    {
        /// <summary>Whether ad breaks are enabled for the platform.</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>Selected provider type. Values: Fake, StaticVast.</summary>
        public string Provider { get; set; } = "Fake";
    }
}
