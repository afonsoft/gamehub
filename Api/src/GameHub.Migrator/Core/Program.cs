using Abp;
using Abp.Castle.Logging.Log4Net;
using Abp.Collections.Extensions;
using Abp.Dependency;
using Castle.Facilities.Logging;
using System;

namespace GameHub.Migrator
{
    public static class Program
    {
        private static bool _skipConnVerification;
        private static string _environment = "LOCAL";

        public static void Main(string[] args)
        {
            ParseArgs(args);

            bool.TryParse(Environment.GetEnvironmentVariable("ASPNETCORE_Docker_Enabled"), out bool isDockerEnabled);

            Environment.SetEnvironmentVariable("EafMigrator", _environment);

            using (var bootstrapper = AbpBootstrapper.Create<MigratorModule>())
            {
                bootstrapper.IocManager.IocContainer
                    .AddFacility<LoggingFacility>(f => f.UseAbpLog4Net()
                        .WithConfig("log4net.config")
                    );

                bootstrapper.Initialize();

                using (var migrateExecuter = bootstrapper.IocManager.ResolveAsDisposable<MigrateExecuter>())
                {
                    migrateExecuter.Object.Run(_skipConnVerification, isDockerEnabled);
                }

                if (_skipConnVerification || isDockerEnabled) return;

                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();
            }
        }

        private static void ParseArgs(string[] args)
        {
            if (args.IsNullOrEmpty())
                return;

            foreach (var arg in args)
            {
                if (arg == "-s")
                    _skipConnVerification = true;

                if (arg == "Development" || arg == "Staging" || arg == "Production")
                    _environment = arg;
            }
        }
    }
}