using Microsoft.Extensions.Configuration;

namespace GameHub.Web.Multiplayer
{
    /// <summary>
    /// Resolves the optional SignalR Redis backplane configuration.
    /// </summary>
    public class SignalRBackplaneSettings
    {
        public bool RedisEnabled { get; private set; }

        public bool IsEnabled { get; private set; }

        public string ConnectionString { get; private set; }

        public string ChannelPrefix { get; private set; }

        public bool CanConfigure =>
            RedisEnabled
            && IsEnabled
            && !string.IsNullOrWhiteSpace(ConnectionString);

        public static SignalRBackplaneSettings FromConfiguration(
            IConfiguration configuration)
        {
            var redisEnabled =
                bool.TryParse(configuration["RedisCache:IsEnabled"], out var redisConfigured)
                && redisConfigured;
            var backplaneEnabled =
                bool.TryParse(
                    configuration["SignalR:Backplane:IsEnabled"],
                    out var backplaneConfigured)
                && backplaneConfigured;

            return new SignalRBackplaneSettings
            {
                RedisEnabled = redisEnabled,
                IsEnabled = backplaneEnabled,
                ConnectionString = configuration["RedisCache:ConnectionString"],
                ChannelPrefix =
                    configuration["SignalR:Backplane:ChannelPrefix"]
                    ?? "gamehub:signalr"
            };
        }
    }
}
