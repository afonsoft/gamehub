using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace GameHub.Web.Configuration
{
    public static class CorsConfiguration
    {
        public const string HubPolicy = "GameHubCors";
        public const string AdminPolicy = "GameHubAdminCors";

        public static IServiceCollection AddGameHubCors(this IServiceCollection services, IConfiguration configuration)
        {
            var hubOrigins = configuration.GetSection("Cors:HubOrigins").Get<string[]>()
                ?? new[] { "https://gamehub.afonsoft.dev", "http://localhost:4200" };

            var adminOrigins = configuration.GetSection("Cors:AdminOrigins").Get<string[]>()
                ?? new[] { "https://gamehub-admin.afonsoft.dev", "http://localhost:4201" };

            var allOrigins = hubOrigins.Concat(adminOrigins).Distinct().ToArray();

            services.AddCors(options =>
            {
                options.AddPolicy(GameHubConsts.DefaultCorsPolicyName, policy =>
                {
                    policy.WithOrigins(CleanOrigins(allOrigins))
                        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                        .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With", "X-Correlation-ID")
                        .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset")
                        .AllowCredentials()
                        .SetPreflightMaxAge(TimeSpan.FromSeconds(600));
                });

                options.AddPolicy(HubPolicy, policy =>
                {
                    policy.WithOrigins(CleanOrigins(hubOrigins))
                        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                        .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With", "X-Correlation-ID")
                        .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset")
                        .AllowCredentials()
                        .SetPreflightMaxAge(TimeSpan.FromSeconds(600));
                });

                options.AddPolicy(AdminPolicy, policy =>
                {
                    policy.WithOrigins(CleanOrigins(adminOrigins))
                        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                        .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With", "X-Correlation-ID")
                        .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset")
                        .AllowCredentials()
                        .SetPreflightMaxAge(TimeSpan.FromSeconds(600));
                });
            });

            return services;
        }

        private static string[] CleanOrigins(string[] origins)
        {
            return origins
                .Select(o => o.Trim())
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select(o => o.TrimEnd('/'))
                .Distinct()
                .ToArray();
        }
    }
}
