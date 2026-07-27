using System;
using System.Threading;
using System.Threading.Tasks;
using GameHub.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GameHub.Web.HealthChecks
{
    /// <summary>
    /// Verifies that the GameHub database context can connect and execute a query.
    /// </summary>
    public class GameHubDbContextHealthCheck : IHealthCheck
    {
        private readonly GameHubDbContext _dbContext;

        public GameHubDbContextHealthCheck(GameHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
                return HealthCheckResult.Healthy("Database connection succeeded.");
            }
            catch (Exception exception)
            {
                return HealthCheckResult.Unhealthy("Database connection failed.", exception);
            }
        }
    }
}
