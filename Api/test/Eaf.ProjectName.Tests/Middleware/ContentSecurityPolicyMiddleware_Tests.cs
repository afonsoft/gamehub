using Eaf.ProjectName.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Eaf.ProjectName.Tests.Middleware
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
    }
}
