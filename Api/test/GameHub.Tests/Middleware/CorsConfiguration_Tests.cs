using Eaf.Middleware.Web.Startup;
using GameHub;
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
        public void Dado_ConfiguracaoPadrao_Quando_AdicionarEafCors_Entao_DeveRegistrarPoliticaPadrao()
        {
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "App:CorsOrigins", "https://gamehub.afonsoft.dev" }
                })
                .Build();

            services.AddEafCors(config, isDevelopment: false, GameHubConsts.DefaultCorsPolicyName);

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CorsOptions>>().Value;

            options.GetPolicy(GameHubConsts.DefaultCorsPolicyName).ShouldNotBeNull();
        }

        [Fact]
        public void Dado_OrigemAdminComCredenciais_Quando_PoliticaPadrao_Entao_DevePermitirSemWildcard()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "App:CorsOrigins", "https://gamehub.afonsoft.dev;https://gamehub-admin.afonsoft.dev" }
                })
                .Build();

            var services = new ServiceCollection();
            services.AddEafCors(config, isDevelopment: false, GameHubConsts.DefaultCorsPolicyName);

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CorsOptions>>().Value;
            var policy = options.GetPolicy(GameHubConsts.DefaultCorsPolicyName);

            policy.ShouldNotBeNull();
            policy.SupportsCredentials.ShouldBeTrue();
            policy.IsOriginAllowed("https://gamehub-admin.afonsoft.dev").ShouldBeTrue();
            policy.Origins.ShouldNotContain("*");
        }

        [Fact]
        public void Dado_WildcardSubdominio_Quando_PoliticaPadrao_Entao_DevePermitirSubdominios()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "App:CorsOrigins", "https://*.afonsoft.dev" }
                })
                .Build();

            var services = new ServiceCollection();
            services.AddEafCors(config, isDevelopment: false, GameHubConsts.DefaultCorsPolicyName);

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CorsOptions>>().Value;
            var policy = options.GetPolicy(GameHubConsts.DefaultCorsPolicyName);

            policy.ShouldNotBeNull();
            policy.IsOriginAllowed("https://gamehub-admin.afonsoft.dev").ShouldBeTrue();
            policy.IsOriginAllowed("https://gamehub.afonsoft.dev").ShouldBeTrue();
        }

        [Fact]
        public void Dado_Desenvolvimento_Quando_CorsOriginsAsterisco_Entao_DevePermitirQualquerOrigem()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "App:CorsOrigins", "*" }
                })
                .Build();

            var services = new ServiceCollection();
            services.AddEafCors(config, isDevelopment: true, GameHubConsts.DefaultCorsPolicyName);

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CorsOptions>>().Value;
            var policy = options.GetPolicy(GameHubConsts.DefaultCorsPolicyName);

            policy.ShouldNotBeNull();
            policy.SupportsCredentials.ShouldBeTrue();
            policy.IsOriginAllowed("https://any-origin.example.com").ShouldBeTrue();
            policy.Origins.ShouldNotContain("*");
        }

        [Fact]
        public void Dado_PoliticaPadrao_Quando_AdicionarEafCors_Entao_DevePermitirHeadersDoEafHttpInterceptor()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "App:CorsOrigins", "https://gamehub.afonsoft.dev" }
                })
                .Build();

            var services = new ServiceCollection();
            services.AddEafCors(config, isDevelopment: false, GameHubConsts.DefaultCorsPolicyName);

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CorsOptions>>().Value;
            var policy = options.GetPolicy(GameHubConsts.DefaultCorsPolicyName);

            policy.ShouldNotBeNull();
            policy.Headers.ShouldContain("Pragma");
            policy.Headers.ShouldContain("Cache-Control");
            policy.Headers.ShouldContain("Expires");
            policy.Headers.ShouldContain("Abp-TenantId");
        }
    }
}
