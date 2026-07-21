using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace GameHub.Web.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        private const int MaxRequests = 100;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

        public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var key = GetClientKey(httpContext);

            if (_cache.TryGetValue(key, out int count))
            {
                if (count >= MaxRequests)
                {
                    httpContext.Response.StatusCode = 429;
                    await httpContext.Response.WriteAsync("Rate limit exceeded.");
                    return;
                }

                _cache.Set(key, count + 1, Window);
            }
            else
            {
                _cache.Set(key, 1, Window);
            }

            await _next.Invoke(httpContext);
        }

        private static string GetClientKey(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "/";
            return $"rate-limit:{ip}:{path}";
        }
    }
}
