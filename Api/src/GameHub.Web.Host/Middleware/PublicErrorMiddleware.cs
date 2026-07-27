using GameHub;
using GameHub.Dto;
using GameHub.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameHub.Web.Middleware
{
    /// <summary>
    /// Captures unhandled exceptions and returns a safe SdkError envelope.
    /// Must be registered after UseExceptionHandler so only unmapped exceptions are handled here.
    /// </summary>
    public class PublicErrorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PublicErrorMiddleware> _logger;

        public PublicErrorMiddleware(RequestDelegate next, ILogger<PublicErrorMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (GameHubException)
            {
                // Already handled by GameHubExceptionFilter or inner middleware
                throw;
            }
            catch (Exception ex)
            {
                var correlationId = context.TraceIdentifier;
                _logger.LogError(ex,
                    "Unhandled exception caught by PublicErrorMiddleware. CorrelationId: {CorrelationId}, Path: {RequestPath}, TenantId: {TenantId}, UserId: {UserId}",
                    correlationId,
                    context.Request.Path,
                    context.Items["Abp.TenantId"],
                    context.User?.Identity?.Name ?? "anonymous");

                await WriteErrorResponseAsync(context, ex, correlationId);
            }
        }

        private static async Task WriteErrorResponseAsync(HttpContext context, Exception ex, string correlationId)
        {
            if (context.Response.HasStarted)
            {
                // Headers already sent; cannot write a new response
                throw new InvalidOperationException("Response has already started; cannot write error envelope.", ex);
            }

            var sdkError = MapToSdkError(ex, correlationId);

            context.Response.Clear();
            context.Response.StatusCode = sdkError.Code == GameHubErrorCodes.ValidationFailed
                ? StatusCodes.Status400BadRequest
                : StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(sdkError), Encoding.UTF8);
        }

        private static SdkError MapToSdkError(Exception ex, string correlationId)
        {
            return ex switch
            {
                ArgumentException or ArgumentNullException or FormatException => new SdkError
                {
                    Code = GameHubErrorCodes.ValidationFailed,
                    Message = "Invalid request. Please check your input and try again.",
                    Retryable = false,
                    CorrelationId = correlationId
                },
                InvalidOperationException => new SdkError
                {
                    Code = GameHubErrorCodes.TemporarilyUnavailable,
                    Message = "The requested operation could not be completed. Please try again later.",
                    Retryable = true,
                    CorrelationId = correlationId
                },
                TimeoutException => new SdkError
                {
                    Code = GameHubErrorCodes.TemporarilyUnavailable,
                    Message = "The operation timed out. Please try again later.",
                    Retryable = true,
                    CorrelationId = correlationId
                },
                _ => new SdkError
                {
                    Code = GameHubErrorCodes.TemporarilyUnavailable,
                    Message = "An unexpected error occurred. Please try again later.",
                    Retryable = true,
                    CorrelationId = correlationId
                }
            };
        }
    }
}
