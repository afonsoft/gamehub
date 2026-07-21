using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GameHub.Web.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var headers = httpContext.Response.Headers;

            AddHeaderIfNotExists(headers, "X-Content-Type-Options", "nosniff");
            AddHeaderIfNotExists(headers, "X-Frame-Options", "DENY");
            AddHeaderIfNotExists(headers, "Referrer-Policy", "strict-origin-when-cross-origin");
            AddHeaderIfNotExists(headers, "Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
            AddHeaderIfNotExists(headers, "X-XSS-Protection", "1; mode=block");
            AddHeaderIfNotExists(headers, "X-LGPD-Compliance", "true");

            await _next.Invoke(httpContext);
        }

        private static void AddHeaderIfNotExists(IHeaderDictionary headers, string key, string value)
        {
            if (!headers.ContainsKey(key))
            {
                headers.Append(key, value);
            }
        }
    }
}
