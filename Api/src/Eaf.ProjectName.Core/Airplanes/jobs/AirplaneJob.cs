using Abp.BackgroundJobs;
using Abp.Timing;
using Hangfire.Console;
using Hangfire.Server;
using System.Threading;
using System.Threading.Tasks;

namespace Eaf.ProjectName.Airplanes.Jobs
{
    public class AirplaneJob : Eaf.BackgroundJobs.AsyncBackgroundJob<string>, IAirplaneJob
    {
        private readonly IBackgroundJobManager _backgroundJobManager;

        public AirplaneJob(
            IBackgroundJobManager backgroundJobManager
        )
        {
            _backgroundJobManager = backgroundJobManager;
        }

        public override Task ExecuteAsync(string args, PerformContext context, CancellationToken token)
        {
            context.WriteLine("Start Job");
            context.WriteLine($"Print args: {args}");
            context.WriteLine("End Job");
            return Task.CompletedTask;
        }

        public Task StartProcess()
        {
            return _backgroundJobManager.EnqueueAsync<AirplaneJob, string>($"Test Job Args {Clock.Now:dd/MM/yyyy}");
        }
    }
}
