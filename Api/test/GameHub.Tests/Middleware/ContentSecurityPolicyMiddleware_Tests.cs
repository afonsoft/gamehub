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
    public class ContentSecurityPolicyMiddleware_Tests
    {
        [Fact]
        public async Task Dado_Middleware_Quando_Invocar_Entao_DeveAdicionarHeaderCSP()
        {
            var context = new DefaultHttpContext();
            var nextCalled = false;
            var middleware = new ContentSecurityPolicyMiddleware(_ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

            await middleware.Invoke(context);

            nextCalled.ShouldBeTrue();
            context.Response.Headers.ContainsKey("Content-Security-Policy").ShouldBeTrue();
            context.Response.Headers.ContainsKey("X-Content-Security-Policy").ShouldBeTrue();
        }

        [Fact]
        public async Task Dado_Middleware_Quando_HeaderJaExiste_Entao_NaoDeveSubstituir()
        {
            var context = new DefaultHttpContext();
            context.Response.Headers["Content-Security-Policy"] = "existing-policy";
            context.Response.Headers["X-Content-Security-Policy"] = "existing-x-policy";

            var middleware = new ContentSecurityPolicyMiddleware(_ => Task.CompletedTask);

            await middleware.Invoke(context);

            context.Response.Headers["Content-Security-Policy"].ToString().ShouldBe("existing-policy");
            context.Response.Headers["X-Content-Security-Policy"].ToString().ShouldBe("existing-x-policy");
        }

        [Fact]
        public async Task Dado_Middleware_Quando_Invocar_Entao_DevePassarParaProximoMiddleware()
        {
            var context = new DefaultHttpContext();
            var nextCalled = false;
            var middleware = new ContentSecurityPolicyMiddleware(_ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

            await middleware.Invoke(context);

            nextCalled.ShouldBeTrue();
        }

        [Fact]
        public async Task Dado_Middleware_Quando_CSPAdicionado_Entao_DeveConterDefaultSrc()
        {
            var context = new DefaultHttpContext();
            var middleware = new ContentSecurityPolicyMiddleware(_ => Task.CompletedTask);

            await middleware.Invoke(context);

            var csp = context.Response.Headers["Content-Security-Policy"].ToString();
            csp.ShouldContain("default-src");
        }

        [Fact]
        public async Task Dado_AmbienteDesenvolvimento_Quando_Invocar_Entao_DeveUsarReportOnly()
        {
            var context = new DefaultHttpContext();
            var env = new HostingEnvironment { EnvironmentName = Environments.Development };
            var middleware = new ContentSecurityPolicyMiddleware(_ => Task.CompletedTask, env);

            await middleware.Invoke(context);

            context.Response.Headers.ContainsKey("Content-Security-Policy").ShouldBeFalse();
            context.Response.Headers.ContainsKey("Content-Security-Policy-Report-Only").ShouldBeTrue();
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
