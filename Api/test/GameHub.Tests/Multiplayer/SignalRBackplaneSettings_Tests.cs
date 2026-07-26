using System.Collections.Generic;
using GameHub.Web.Multiplayer;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Multiplayer
{
    public class SignalRBackplaneSettings_Tests
    {
        [Fact]
        public void Dado_RedisEBackplaneHabilitados_Quando_LerConfiguracao_Entao_PodeConfigurar()
        {
            var settings = CreateSettings(
                redisEnabled: true,
                backplaneEnabled: true,
                connectionString: "redis:6379",
                channelPrefix: "gamehub:test");

            settings.CanConfigure.ShouldBeTrue();
            settings.ChannelPrefix.ShouldBe("gamehub:test");
        }

        [Fact]
        public void Dado_RedisDesabilitado_Quando_LerConfiguracao_Entao_NaoPodeConfigurar()
        {
            var settings = CreateSettings(
                redisEnabled: false,
                backplaneEnabled: true,
                connectionString: "redis:6379",
                channelPrefix: "gamehub:test");

            settings.CanConfigure.ShouldBeFalse();
        }

        [Fact]
        public void Dado_PrefixoAusente_Quando_LerConfiguracao_Entao_UsaPrefixoPadrao()
        {
            var settings = CreateSettings(
                redisEnabled: true,
                backplaneEnabled: true,
                connectionString: "redis:6379",
                channelPrefix: null);

            settings.ChannelPrefix.ShouldBe("gamehub:signalr");
        }

        private static SignalRBackplaneSettings CreateSettings(
            bool redisEnabled,
            bool backplaneEnabled,
            string connectionString,
            string channelPrefix)
        {
            var values = new Dictionary<string, string>
            {
                ["RedisCache:IsEnabled"] = redisEnabled.ToString(),
                ["RedisCache:ConnectionString"] = connectionString,
                ["SignalR:Backplane:IsEnabled"] = backplaneEnabled.ToString(),
                ["SignalR:Backplane:ChannelPrefix"] = channelPrefix
            };

            return SignalRBackplaneSettings.FromConfiguration(
                new ConfigurationBuilder()
                    .AddInMemoryCollection(values)
                    .Build());
        }
    }
}
