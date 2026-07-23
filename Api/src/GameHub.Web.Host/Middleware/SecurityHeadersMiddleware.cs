using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace GameHub.Web.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _environment;

        public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
        {
            _next = next;
            _environment = environment;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var headers = httpContext.Response.Headers;

            AddHeaderIfNotExists(headers, "X-Content-Type-Options", "nosniff");
            AddHeaderIfNotExists(headers, "X-Frame-Options", GetFrameOptions(httpContext));
            AddHeaderIfNotExists(headers, "X-XSS-Protection", "0");
            AddHeaderIfNotExists(headers, "Referrer-Policy", "strict-origin-when-cross-origin");
            AddHeaderIfNotExists(headers, "Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
            AddHeaderIfNotExists(headers, "X-Permitted-Cross-Domain-Policies", "none");
            AddHeaderIfNotExists(headers, "Cross-Origin-Resource-Policy", "same-site");
            AddHeaderIfNotExists(headers, "X-LGPD-Compliance", "true");

            if (!_environment.IsDevelopment())
            {
                AddHeaderIfNotExists(headers, "Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
            }

            headers.Remove("Server");
            headers.Remove("X-Powered-By");

            await _next.Invoke(httpContext);
        }

        private static string GetFrameOptions(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/play"))
            {
                return "SAMEORIGIN";
            }

            return "DENY";
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
