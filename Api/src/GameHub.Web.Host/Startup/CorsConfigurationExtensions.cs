using Eaf.Middleware.Web.Startup;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GameHub.Web.Startup
{
    /// <summary>
    /// Extends EAF CORS registration with GameHub-specific headers (e.g. SignalR).
    /// </summary>
    public static class CorsConfigurationExtensions
    {
        /// <summary>
        /// Adds the default EAF CORS policy and extends it with headers required by
        /// <c>@microsoft/signalr</c> (X-SignalR-User-Agent).
        /// </summary>
        public static IServiceCollection AddGameHubCors(
            this IServiceCollection services,
            IConfiguration configuration,
            bool isDevelopment,
            string policyName)
        {
            services.AddEafCors(configuration, isDevelopment, policyName);

            services.PostConfigure<CorsOptions>(options =>
            {
                var policy = options.GetPolicy(policyName);
                if (policy != null)
                {
                    policy.Headers.Add("X-SignalR-User-Agent");
                }
            });

            return services;
        }
    }
}
