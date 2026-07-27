using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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

            var legacyOrigins = (configuration["App:CorsOrigins"] ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim())
                .Where(o => !string.IsNullOrWhiteSpace(o));

            var allOrigins = hubOrigins
                .Concat(adminOrigins)
                .Concat(legacyOrigins)
                .Select(NormalizeOrigin)
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var hubSet = new HashSet<string>(hubOrigins.Select(NormalizeOrigin).Where(o => !string.IsNullOrWhiteSpace(o)), StringComparer.OrdinalIgnoreCase);
            var adminSet = new HashSet<string>(adminOrigins.Select(NormalizeOrigin).Where(o => !string.IsNullOrWhiteSpace(o)), StringComparer.OrdinalIgnoreCase);

            var allowAnyOrigin = configuration.GetValue<bool?>("Cors:AllowAnyOrigin") ?? false;

            services.AddCors(options =>
            {
                options.AddPolicy(GameHubConsts.DefaultCorsPolicyName, policy =>
                {
                    ConfigurePolicy(policy, allOrigins, allowAnyOrigin);
                });

                options.AddPolicy(HubPolicy, policy =>
                {
                    ConfigurePolicy(policy, hubSet.ToArray(), allowAnyOrigin);
                });

                options.AddPolicy(AdminPolicy, policy =>
                {
                    ConfigurePolicy(policy, adminSet.ToArray(), allowAnyOrigin);
                });
            });

            return services;
        }

        private static void ConfigurePolicy(Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicyBuilder policy, string[] origins, bool allowAnyOrigin)
        {
            if (allowAnyOrigin || origins.Length == 0)
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(600));
                return;
            }

            policy.SetIsOriginAllowed(origin => IsOriginAllowed(origin, origins))
                .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                .WithHeaders(
                    "Authorization",
                    "Content-Type",
                    "Accept",
                    "X-Requested-With",
                    "X-Correlation-ID",
                    "Abp.TenantId",
                    "Abp.Localization.CultureName",
                    ".AspNetCore.Culture",
                    "Accept-Language")
                .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset", "Retry-After")
                .AllowCredentials()
                .SetPreflightMaxAge(TimeSpan.FromSeconds(600));
        }

        private static bool IsOriginAllowed(string origin, IEnumerable<string> allowedPatterns)
        {
            if (string.IsNullOrWhiteSpace(origin))
            {
                return false;
            }

            var normalized = NormalizeOrigin(origin);

            foreach (var pattern in allowedPatterns)
            {
                if (pattern == "*")
                {
                    return true;
                }

                if (string.Equals(normalized, pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (IsWildcardMatch(normalized, pattern))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsWildcardMatch(string origin, string pattern)
        {
            // Supports patterns like https://*.afonsoft.com.br
            if (!pattern.Contains("*"))
            {
                return false;
            }

            var escaped = System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".+");
            var regex = $"^{escaped}$";
            return System.Text.RegularExpressions.Regex.IsMatch(origin, regex, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private static string NormalizeOrigin(string origin)
        {
            if (string.IsNullOrWhiteSpace(origin))
            {
                return string.Empty;
            }

            var value = origin.Trim();
            if (!value.Contains("://"))
            {
                return value.ToLowerInvariant();
            }

            if (Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                var builder = new UriBuilder(uri.Scheme.ToLowerInvariant(), uri.Host.ToLowerInvariant())
                {
                    Port = uri.IsDefaultPort ? -1 : uri.Port
                };
                return builder.Uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
            }

            return value.ToLowerInvariant();
        }
    }
}
