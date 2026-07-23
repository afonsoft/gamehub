using GameHub.Web.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace GameHub.Tests.Middleware
{
    public class SecurityHeadersMiddleware_Tests
    {
        [Fact]
        public async Task Dado_RequisicaoNormal_Quando_Invocar_Entao_DeveAdicionarSecurityHeaders()
        {
            var context = new DefaultHttpContext();
            var env = new HostingEnvironment { EnvironmentName = Environments.Production };
            var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask, env);

            await middleware.Invoke(context);

            context.Response.Headers["X-Content-Type-Options"].ToString().ShouldBe("nosniff");
            context.Response.Headers["X-Frame-Options"].ToString().ShouldBe("DENY");
            context.Response.Headers["X-XSS-Protection"].ToString().ShouldBe("0");
            context.Response.Headers["Referrer-Policy"].ToString().ShouldBe("strict-origin-when-cross-origin");
            context.Response.Headers["Strict-Transport-Security"].ToString().ShouldBe("max-age=31536000; includeSubDomains; preload");
            context.Response.Headers.ContainsKey("Server").ShouldBeFalse();
        }

        [Fact]
        public async Task Dado_RotaPlay_Quando_Invocar_Entao_XFrameOptionsDeveSerSameOrigin()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/play/some-game";
            var env = new HostingEnvironment { EnvironmentName = Environments.Production };
            var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask, env);

            await middleware.Invoke(context);

            context.Response.Headers["X-Frame-Options"].ToString().ShouldBe("SAMEORIGIN");
        }

        [Fact]
        public async Task Dado_AmbienteDesenvolvimento_Quando_Invocar_Entao_NaoDeveAdicionarHSTS()
        {
            var context = new DefaultHttpContext();
            var env = new HostingEnvironment { EnvironmentName = Environments.Development };
            var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask, env);

            await middleware.Invoke(context);

            context.Response.Headers.ContainsKey("Strict-Transport-Security").ShouldBeFalse();
        }

        private class HostingEnvironment : IWebHostEnvironment
        {
            public string EnvironmentName { get; set; }
            public string ApplicationName { get; set; } = "GameHub";
            public string ContentRootPath { get; set; } = string.Empty;
            public IFileProvider ContentRootFileProvider { get; set; }
            public string WebRootPath { get; set; } = string.Empty;
            public IFileProvider WebRootFileProvider { get; set; }
        }
    }
}
