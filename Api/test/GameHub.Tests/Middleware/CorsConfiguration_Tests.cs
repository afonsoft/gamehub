using GameHub.Web.Configuration;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
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
    }
}
