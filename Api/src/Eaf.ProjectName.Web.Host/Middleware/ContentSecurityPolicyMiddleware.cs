using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Eaf.ProjectName.Web.Middleware
{
    public class ContentSecurityPolicyMiddleware
    {
        private readonly RequestDelegate _next;

        private const string ContentSecurityPolicy = "default-src * 'self' https://*.google-analytics.com https://*.analytics.google.com https://*.googletagmanager.com https://*.g.doubleclick.net https://*.google.com https://*.hotjar.com https://*.hotjar.io wss://*.hotjar.com https://*.dynatrace.com 'unsafe-inline' 'unsafe-eval'; img-src * 'self' data: https:;";

        public ContentSecurityPolicyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            //Content-Security-Policy is the name of a HTTP response header that modern browsers use to enhance the security of the document (or web page). The Content-Security-Policy header allows you to restrict how resources such as JavaScript, CSS, or pretty much anything that the browser loads.
            AddHeaderIfNotExists(httpContext, "X-Content-Security-Policy", ContentSecurityPolicy);

            //Content-Security-Policy is the name of a HTTP response header that modern browsers use to enhance the security of the document (or web page). The Content-Security-Policy header allows you to restrict how resources such as JavaScript, CSS, or pretty much anything that the browser loads.
            AddHeaderIfNotExists(httpContext, "Content-Security-Policy", ContentSecurityPolicy);

            await _next.Invoke(httpContext);
        }

        private static void AddHeaderIfNotExists(HttpContext context, string key, string value)
        {
            if (context?.Response != null && !context.Response.Headers.ContainsKey(key))
            {
                context.Response.Headers.Append(key, value);
            }
        }
    }
}