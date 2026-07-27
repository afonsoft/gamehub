using GameHub;
using GameHub.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameHub.Web.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        public RateLimitingMiddleware(RequestDelegate next, IDistributedCache cache, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Options)
            {
                await _next(context);
                return;
            }

            var rule = ResolveRule(context);
            var partitionKey = rule.GetPartitionKey(context);
            var bucket = GetBucket(rule.Window);
            var key = $"rate-limit:{rule.Name}:{partitionKey}:{bucket}";

            var count = await GetCountAsync(key);
            var limit = rule.Limit;
            var reset = bucket + rule.Window;

            if (count >= limit)
            {
                _logger.LogWarning("Rate limit exceeded for rule {Rule} partition {Partition}", rule.Name, partitionKey);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";

                var retryAfter = Math.Max(1, (int)(reset - DateTimeOffset.UtcNow).TotalSeconds);
                context.Response.Headers["Retry-After"] = retryAfter.ToString();

                var response = new SdkError
                {
                    Code = GameHubErrorCodes.RateLimited,
                    Message = "Too many requests. Please try again later.",
                    Retryable = true,
                    CorrelationId = context.TraceIdentifier
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response), Encoding.UTF8);
                return;
            }

            await SetCountAsync(key, count + 1, rule.Window);

            var remaining = Math.Max(0, limit - count - 1);
            AddRateLimitHeaders(context, limit, remaining, reset);

            await _next(context);
        }

        private static RateLimitRule ResolveRule(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;

            if (path.StartsWith("/api/TokenAuth", StringComparison.OrdinalIgnoreCase))
                return RateLimitRules.Auth;

            if (path.StartsWith("/api/services/app/Gameplay", StringComparison.OrdinalIgnoreCase))
                return RateLimitRules.Gameplay;

            if (path.StartsWith("/api/gamebuilds", StringComparison.OrdinalIgnoreCase) && method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
                return RateLimitRules.Upload;

            return RateLimitRules.Default;
        }

        private static string GetPartitionKey(HttpContext context, bool perSession)
        {
            if (perSession)
            {
                var sessionId = context.Request.Headers["X-Session-Id"].ToString();
                if (!string.IsNullOrWhiteSpace(sessionId))
                    return $"session:{sessionId}";
            }

            var userId = context.User?.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(userId))
                return $"user:{userId}";

            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return $"ip:{ip}";
        }

        private static string GetUserPartitionKey(HttpContext context)
        {
            var userId = context.User?.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(userId))
                return $"user:{userId}";

            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return $"ip:{ip}";
        }

        private static DateTimeOffset GetBucket(TimeSpan window)
        {
            var now = DateTimeOffset.UtcNow;
            var ticks = now.Ticks / window.Ticks * window.Ticks;
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

        private async Task<long> GetCountAsync(string key)
        {
            var value = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(value))
                return 0;

            return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) ? count : 0;
        }

        private async Task SetCountAsync(string key, long count, TimeSpan window)
        {
            await _cache.SetStringAsync(
                key,
                count.ToString(CultureInfo.InvariantCulture),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = window
                });
        }

        private static void AddRateLimitHeaders(HttpContext context, long limit, long remaining, DateTimeOffset reset)
        {
            var headers = context.Response.Headers;
            headers["X-RateLimit-Limit"] = limit.ToString();
            headers["X-RateLimit-Remaining"] = remaining.ToString();
            headers["X-RateLimit-Reset"] = reset.ToUnixTimeSeconds().ToString();
        }

        private class RateLimitRule
        {
            public string Name { get; set; }
            public int Limit { get; set; }
            public TimeSpan Window { get; set; }
            public Func<HttpContext, string> GetPartitionKey { get; set; }
        }

        private static class RateLimitRules
        {
            public static readonly RateLimitRule Auth = new()
            {
                Name = "auth",
                Limit = 10,
                Window = TimeSpan.FromMinutes(1),
                GetPartitionKey = ctx => GetPartitionKey(ctx, perSession: false)
            };

            public static readonly RateLimitRule Gameplay = new()
            {
                Name = "gameplay",
                Limit = 60,
                Window = TimeSpan.FromMinutes(1),
                GetPartitionKey = ctx => GetPartitionKey(ctx, perSession: true)
            };

            public static readonly RateLimitRule Upload = new()
            {
                Name = "upload",
                Limit = 5,
                Window = TimeSpan.FromHours(1),
                GetPartitionKey = ctx => GetUserPartitionKey(ctx)
            };

            public static readonly RateLimitRule Default = new()
            {
                Name = "default",
                Limit = 100,
                Window = TimeSpan.FromMinutes(1),
                GetPartitionKey = ctx => GetPartitionKey(ctx, perSession: false)
            };
        }
    }
}
