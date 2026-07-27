using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Abp.Runtime.Security;

namespace GameHub.Web.Extensions
{
    public static class ClaimExtensions
    {
        public static long? GetUserIdFromClaims(this IEnumerable<Claim> claims)
        {
            var value = claims
                .FirstOrDefault(c => c.Type == AbpClaimTypes.UserId
                                     || c.Type == ClaimTypes.NameIdentifier
                                     || c.Type == "sub")?.Value;

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return long.TryParse(value, out var userId) ? userId : null;
        }

        public static int? GetTenantIdFromClaims(this IEnumerable<Claim> claims)
        {
            var value = claims
                .FirstOrDefault(c => c.Type == AbpClaimTypes.TenantId
                                     || c.Type == "tenantid")?.Value;

            if (string.IsNullOrWhiteSpace(value) || value == "0")
            {
                return null;
            }

            return int.TryParse(value, out var tenantId) ? tenantId : null;
        }

        public static void AddOrReplaceClaim(this List<Claim> claims, Claim claim)
        {
            claims.RemoveAll(c => c.Type == claim.Type);
            claims.Add(claim);
        }
    }
}
