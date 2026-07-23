using GameHub.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace GameHub.Tests.Middleware
{
    public class RateLimitingMiddleware_Tests
    {
        [Fact]
        public async Task Dado_PrimeiraRequisicao_Quando_Invocar_Entao_DeveAdicionarRateLimitHeaders()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/services/app/GameCatalog/GetHome";
            var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            var middleware = new RateLimitingMiddleware(_ => Task.CompletedTask, cache, NullLogger<RateLimitingMiddleware>.Instance);

            await middleware.Invoke(context);

            context.Response.Headers["X-RateLimit-Limit"].ToString().ShouldBe("100");
            context.Response.Headers["X-RateLimit-Remaining"].ToString().ShouldBe("99");
            context.Response.Headers.ContainsKey("X-RateLimit-Reset").ShouldBeTrue();
            context.Response.StatusCode.ShouldBe(200);
        }

        [Fact]
        public async Task Dado_ExcederLimiteDefault_Quando_Invocar_Entao_DeveRetornar429()
        {
            var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            var middleware = new RateLimitingMiddleware(_ => Task.CompletedTask, cache, NullLogger<RateLimitingMiddleware>.Instance);

            for (var i = 0; i < 101; i++)
            {
                var context = new DefaultHttpContext();
                context.Request.Path = "/api/services/app/GameCatalog/GetHome";
                context.Response.Body = new System.IO.MemoryStream();
                await middleware.Invoke(context);
            }

            var last = new DefaultHttpContext();
            last.Request.Path = "/api/services/app/GameCatalog/GetHome";
            last.Response.Body = new System.IO.MemoryStream();
            await middleware.Invoke(last);

            last.Response.StatusCode.ShouldBe(429);
            last.Response.Headers["Retry-After"].ToString().ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async Task Dado_RequisicaoOptions_Quando_Invocar_Entao_NaoDeveAplicarRateLimit()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "OPTIONS";
            var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            var middleware = new RateLimitingMiddleware(_ => Task.CompletedTask, cache, NullLogger<RateLimitingMiddleware>.Instance);

            await middleware.Invoke(context);

            context.Response.StatusCode.ShouldBe(200);
        }
    }
}
