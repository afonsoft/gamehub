using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.UI;
using GameHub.Dto;
using GameHub.Exceptions;
using GameHub.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Middleware
{
    public class PublicErrorMiddleware_Tests
    {
        [Fact]
        public async Task Dado_RequisicaoNormal_Quando_NenhumaExcecao_Entao_DeveContinuar()
        {
            var context = new DefaultHttpContext();
            var middleware = new PublicErrorMiddleware(_ => Task.CompletedTask, NullLogger<PublicErrorMiddleware>.Instance);

            await middleware.Invoke(context);

            context.Response.StatusCode.ShouldBe(200);
        }

        [Fact]
        public async Task Dado_ExcecaoArgument_Quando_Invocar_Entao_DeveRetornar400()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var middleware = new PublicErrorMiddleware(_ => throw new ArgumentException("Invalid input"), NullLogger<PublicErrorMiddleware>.Instance);

            await middleware.Invoke(context);

            context.Response.StatusCode.ShouldBe(400);
            context.Response.ContentType.ShouldContain("application/json");
            var body = await ReadBodyAsync(context.Response.Body);
            body.ShouldContain("Invalid request");
        }

        [Fact]
        public async Task Dado_ExcecaoGenerica_Quando_Invocar_Entao_DeveRetornar500ComRetryable()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var middleware = new PublicErrorMiddleware(_ => throw new InvalidOperationException("boom"), NullLogger<PublicErrorMiddleware>.Instance);

            await middleware.Invoke(context);

            context.Response.StatusCode.ShouldBe(500);
            context.Response.ContentType.ShouldContain("application/json");
            var body = await ReadBodyAsync(context.Response.Body);
            body.ShouldContain("Please try again later");
            body.ShouldContain("\"Retryable\":true");
        }

        [Fact]
        public async Task Dado_UserFriendlyException_Quando_Invocar_Entao_DeveRetornar400ComMensagem()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var middleware = new PublicErrorMiddleware(_ => throw new UserFriendlyException("User has no associated tenants"), NullLogger<PublicErrorMiddleware>.Instance);

            await middleware.Invoke(context);

            context.Response.StatusCode.ShouldBe(400);
            context.Response.ContentType.ShouldContain("application/json");
            var body = await ReadBodyAsync(context.Response.Body);
            body.ShouldContain("User has no associated tenants");
            body.ShouldContain("\"Retryable\":false");
        }

        [Fact]
        public async Task Dado_GameHubException_Quando_Invocar_Entao_DevePropagar()
        {
            var context = new DefaultHttpContext();
            var middleware = new PublicErrorMiddleware(_ => throw new GameHubException("gamehub", "boom", false), NullLogger<PublicErrorMiddleware>.Instance);

            await Should.ThrowAsync<GameHubException>(async () => await middleware.Invoke(context));
        }

        private static async Task<string> ReadBodyAsync(Stream stream)
        {
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }
}
