using System;
using System.IO;
using System.Threading.Tasks;
using Abp.UI;
using Eaf.Middleware.Web.Middleware;
using GameHub.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Middleware
{
    public class PublicErrorMiddleware_Tests
    {
        private static DefaultHttpContext CreateContext()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            context.RequestServices = new ServiceCollection()
                .AddControllers()
                .Services
                .BuildServiceProvider();
            return context;
        }

        private static async Task<string> ReadBodyAsync(Stream stream)
        {
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        [Fact]
        public async Task Dado_RequisicaoNormal_Quando_NenhumaExcecao_Entao_DeveContinuar()
        {
            var context = new DefaultHttpContext();
            var middleware = new EafPublicErrorMiddleware(_ => Task.CompletedTask, NullLogger<EafPublicErrorMiddleware>.Instance);

            await middleware.Invoke(context);

            context.Response.StatusCode.ShouldBe(200);
        }

        [Fact]
        public async Task Dado_ExcecaoArgument_Quando_Invocar_Entao_DeveRetornar400()
        {
            var context = CreateContext();
            var middleware = new EafPublicErrorMiddleware(_ => throw new ArgumentException("Invalid input"), NullLogger<EafPublicErrorMiddleware>.Instance);

            await middleware.Invoke(context);

            context.Response.StatusCode.ShouldBe(400);
            context.Response.ContentType.ShouldContain("application/json");
            var body = await ReadBodyAsync(context.Response.Body);
            body.ShouldContain("Invalid request");
        }

        [Fact]
        public async Task Dado_ExcecaoGenerica_Quando_Invocar_Entao_DeveRetornar500ComRetryable()
        {
            var context = CreateContext();
            var middleware = new EafPublicErrorMiddleware(_ => throw new InvalidOperationException("boom"), NullLogger<EafPublicErrorMiddleware>.Instance);

            await middleware.Invoke(context);

            context.Response.StatusCode.ShouldBe(500);
            context.Response.ContentType.ShouldContain("application/json");
            var body = await ReadBodyAsync(context.Response.Body);
            body.ShouldContain("Please try again later");
            body.ShouldContain("\"retryable\":true");
        }

        [Fact]
        public async Task Dado_UserFriendlyException_Quando_Invocar_Entao_DeveRetornar400ComMensagem()
        {
            var context = CreateContext();
            var middleware = new EafPublicErrorMiddleware(_ => throw new UserFriendlyException("User has no associated tenants"), NullLogger<EafPublicErrorMiddleware>.Instance);

            await middleware.Invoke(context);

            context.Response.StatusCode.ShouldBe(400);
            context.Response.ContentType.ShouldContain("application/json");
            var body = await ReadBodyAsync(context.Response.Body);
            body.ShouldContain("User has no associated tenants");
            body.ShouldContain("\"retryable\":false");
        }

        [Fact]
        public async Task Dado_GameHubException_Quando_Invocar_Entao_DeveRetornar500ComRetryable()
        {
            var context = CreateContext();
            var middleware = new EafPublicErrorMiddleware(_ => throw new GameHubException("gamehub", "boom", false), NullLogger<EafPublicErrorMiddleware>.Instance);

            await middleware.Invoke(context);

            context.Response.StatusCode.ShouldBe(500);
            context.Response.ContentType.ShouldContain("application/json");
            var body = await ReadBodyAsync(context.Response.Body);
            body.ShouldContain("An unexpected error occurred");
            body.ShouldContain("\"retryable\":true");
        }
    }
}
