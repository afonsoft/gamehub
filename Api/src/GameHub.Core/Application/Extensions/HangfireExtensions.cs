using GameHub.Airplanes;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using System;

namespace GameHub.Application.Extensions
{
    public static class HangfireExtensions
    {
        public static void ScheduleRecurringJobs(this IApplicationBuilder app)
        {
            RecurringJob.AddOrUpdate<IAirplaneManager>("DateUpdateProcess", x => x.DateUpdate(null), Cron.Minutely, TimeZoneInfo.Local);
        }
    }
}