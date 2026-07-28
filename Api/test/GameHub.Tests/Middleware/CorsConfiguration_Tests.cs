using GameHub;
using GameHub.Web.Configuration;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GameHub.Tests.Middleware
{
    public class CorsConfiguration_Tests
    {
        [Fact]
        public void Dado_ConfiguracaoPadrao_Quando_AdicionarCors_Entao_DeveRegistrarPoliticas()
        {
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder().Build();

            services.AddGameHubCors(config);

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CorsOptions>>().Value;

            options.GetPolicy(CorsConfiguration.HubPolicy).ShouldNotBeNull();
            options.GetPolicy(CorsConfiguration.AdminPolicy).ShouldNotBeNull();
            options.GetPolicy(GameHubConsts.DefaultCorsPolicyName).ShouldNotBeNull();
        }

        [Fact]
        public void Dado_PoliticaPadrao_Quando_OrigemAdminComCredenciais_Entao_DevePermitirSemWildcard()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Cors:AllowAnyOrigin", "false" },
                    { "Cors:HubOrigins:0", "https://gamehub.afonsoft.dev" },
                    { "Cors:AdminOrigins:0", "https://gamehub-admin.afonsoft.dev" },
                })
                .Build();

            var services = new ServiceCollection();
            services.AddGameHubCors(config);

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CorsOptions>>().Value;
            var policy = options.GetPolicy(GameHubConsts.DefaultCorsPolicyName);

            policy.ShouldNotBeNull();
            policy.SupportsCredentials.ShouldBeTrue();
            policy.IsOriginAllowed("https://gamehub-admin.afonsoft.dev").ShouldBeTrue();
            policy.Origins.ShouldNotContain("*");
        }

        [Fact]
        public void Dado_AllowAnyOriginTrue_Quando_PoliticaPadrao_Entao_DevePermitirQualquerOrigemComCredenciaisSemWildcard()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Cors:AllowAnyOrigin", "true" },
                })
                .Build();

            var services = new ServiceCollection();
            services.AddGameHubCors(config);

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CorsOptions>>().Value;
            var policy = options.GetPolicy(GameHubConsts.DefaultCorsPolicyName);

            policy.ShouldNotBeNull();
            policy.SupportsCredentials.ShouldBeTrue();
            policy.IsOriginAllowed("https://gamehub-admin.afonsoft.dev").ShouldBeTrue();
            policy.IsOriginAllowed("https://gamehub.afonsoft.dev").ShouldBeTrue();
            policy.Origins.ShouldNotContain("*");
        }

        [Fact]
        public void Dado_OrigemComWildcard_Quando_PoliticaPadrao_Entao_DevePermitirSubdominios()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Cors:AllowAnyOrigin", "false" },
                    { "App:CorsOrigins", "https://*.afonsoft.dev" },
                })
                .Build();

            var services = new ServiceCollection();
            services.AddGameHubCors(config);

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CorsOptions>>().Value;
            var policy = options.GetPolicy(GameHubConsts.DefaultCorsPolicyName);

            policy.ShouldNotBeNull();
            policy.IsOriginAllowed("https://gamehub-admin.afonsoft.dev").ShouldBeTrue();
            policy.IsOriginAllowed("https://gamehub.afonsoft.dev").ShouldBeTrue();
        }

        [Fact]
        public void Dado_PoliticaPadrao_Quando_AdicionarCors_Entao_DevePermitirHeadersDoEafHttpInterceptor()
        {
            var config = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();
            services.AddGameHubCors(config);

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CorsOptions>>().Value;
            var policy = options.GetPolicy(GameHubConsts.DefaultCorsPolicyName);

            policy.ShouldNotBeNull();
            policy.Headers.ShouldContain("Pragma");
            policy.Headers.ShouldContain("Cache-Control");
            policy.Headers.ShouldContain("Expires");
        }
    }
}
