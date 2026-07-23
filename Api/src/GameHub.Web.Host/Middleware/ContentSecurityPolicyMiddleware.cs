using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading.Tasks;

namespace GameHub.Web.Middleware
{
    public class ContentSecurityPolicyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _environment;

        public ContentSecurityPolicyMiddleware(RequestDelegate next, IWebHostEnvironment environment = null)
        {
            _next = next;
            _environment = environment;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var isDevelopment = _environment?.IsDevelopment() ?? false;
            var csp = isDevelopment ? BuildDevelopmentCsp() : BuildProductionCsp();

            var headerName = isDevelopment ? "Content-Security-Policy-Report-Only" : "Content-Security-Policy";

            AddHeaderIfNotExists(httpContext, headerName, csp);

            // Kept for backward compatibility with older browsers/tests.
            AddHeaderIfNotExists(httpContext, "X-Content-Security-Policy", csp);

            await _next.Invoke(httpContext);
        }

        private static string BuildProductionCsp() =>
            string.Join("; ", new[]
            {
                "default-src 'self'",
                "script-src 'self'",
                "style-src 'self' 'unsafe-inline'",
                "img-src 'self' data: https://gamehub.afonsoft.dev https://gamehub-api.afonsoft.dev",
                "font-src 'self'",
                "connect-src 'self' https://gamehub-api.afonsoft.dev wss://gamehub-api.afonsoft.dev",
                "frame-src https://games.afonsoft.dev",
                "frame-ancestors 'self' https://gamehub.afonsoft.dev",
                "object-src 'none'",
                "base-uri 'self'",
                "form-action 'self'",
                "upgrade-insecure-requests"
            });

        private static string BuildDevelopmentCsp() =>
            string.Join("; ", new[]
            {
                "default-src 'self'",
                "script-src 'self' 'unsafe-eval' 'unsafe-inline'",
                "style-src 'self' 'unsafe-inline' 'unsafe-eval'",
                "img-src 'self' data: https://gamehub.afonsoft.dev https://gamehub-api.afonsoft.dev",
                "font-src 'self'",
                "connect-src 'self' https://gamehub-api.afonsoft.dev wss://gamehub-api.afonsoft.dev http://localhost:* ws://localhost:*",
                "frame-src https://games.afonsoft.dev",
                "frame-ancestors 'self' https://gamehub.afonsoft.dev",
                "object-src 'none'",
                "base-uri 'self'",
                "form-action 'self'"
            });

        private static void AddHeaderIfNotExists(HttpContext context, string key, string value)
        {
            if (context?.Response != null && !context.Response.Headers.ContainsKey(key))
            {
                context.Response.Headers.Append(key, value);
            }
        }
    }
}
