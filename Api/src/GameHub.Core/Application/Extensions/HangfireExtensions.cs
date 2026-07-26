using GameHub.Jobs;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using System;

namespace GameHub.Application.Extensions
{
    public static class HangfireExtensions
    {
        public static void ScheduleRecurringJobs(this IApplicationBuilder app)
        {
            RecurringJob.AddOrUpdate<GameMetricsAggregationJob>(
                "metrics-aggregation",
                job => job.Execute(new GameMetricsAggregationArgs
                {
                    Date = DateTime.UtcNow.Date.AddDays(-1)
                }),
                Cron.Daily);

            RecurringJob.AddOrUpdate<CleanupExpiredArbitraryUserDataJob>(
                "auds-cleanup-expired",
                job => job.Execute(),
                Cron.Daily);

            RecurringJob.AddOrUpdate<CleanupMultiplayerJob>(
                "multiplayer-cleanup",
                job => job.Execute(),
                Cron.Hourly);
        }
    }
}
