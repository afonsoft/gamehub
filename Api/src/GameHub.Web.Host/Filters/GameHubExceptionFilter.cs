using System;
using GameHub.Dto;
using GameHub.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GameHub.Web.Filters;

/// <summary>
/// Maps GameHubException to the public SdkError contract.
/// </summary>
public class GameHubExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not GameHubException gameHubException)
        {
            return;
        }

        var statusCode = ResolveStatusCode(gameHubException.ErrorCode);
        var sdkError = new SdkError
        {
            Code = gameHubException.ErrorCode,
            Message = gameHubException.Message,
            Retryable = gameHubException.Retryable,
            CorrelationId = context.HttpContext.TraceIdentifier
        };

        context.Result = new ObjectResult(sdkError)
        {
            StatusCode = statusCode,
            DeclaredType = typeof(SdkError)
        };

        if (gameHubException.ErrorCode == GameHubErrorCodes.RateLimited)
        {
            context.HttpContext.Response.Headers.Append("Retry-After", "60");
        }

        context.ExceptionHandled = true;
    }

    private static int ResolveStatusCode(string errorCode)
    {
        return errorCode switch
        {
            GameHubErrorCodes.NotAuthenticated => StatusCodes.Status401Unauthorized,
            GameHubErrorCodes.NotAuthorized or GameHubErrorCodes.FeatureDisabled => StatusCodes.Status403Forbidden,
            GameHubErrorCodes.RateLimited => StatusCodes.Status429TooManyRequests,
            GameHubErrorCodes.TemporarilyUnavailable => StatusCodes.Status503ServiceUnavailable,
            GameHubErrorCodes.ValidationFailed or GameHubErrorCodes.InvalidContext => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
