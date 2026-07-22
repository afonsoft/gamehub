using Hangfire;
using Microsoft.AspNetCore.Builder;
using System;

namespace GameHub.Application.Extensions
{
    public static class HangfireExtensions
    {
        public static void ScheduleRecurringJobs(this IApplicationBuilder app)
        {
            // Recurring jobs for GameHub metrics/aggregations will be scheduled here.
            // Placeholder: no-op until GameMetricsAggregationJob is implemented.
        }
    }
}