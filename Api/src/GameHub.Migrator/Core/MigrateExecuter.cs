using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.MultiTenancy;
using GameHub.EntityFrameworkCore;
using GameHub.Migrations.Seed;
using System;

namespace GameHub.Migrator
{
    public class MigrateExecuter : ITransientDependency
    {
        public Log Log { get; private set; }

        private readonly ProjectNameDbMigrator _migrator;
        private readonly DefaultConnectionStringResolver _connectionStringResolver;

        public MigrateExecuter(
            Log log,
            ProjectNameDbMigrator migrator,
            DefaultConnectionStringResolver connectionStringResolver
        )
        {
            Log = log;
            _migrator = migrator;
            _connectionStringResolver = connectionStringResolver;
        }

        public void Run(bool skipConnVerification, bool isDockerEnabled = false)
        {
            var hostConnStr = _connectionStringResolver.GetNameOrConnectionString(new ConnectionStringResolveArgs(MultiTenancySides.Host));
            if (hostConnStr.IsNullOrWhiteSpace())
            {
                Log.Write("Configuration file should contain a connection string named 'LOCAL'");
                return;
            }

            Log.Write("Database: " + Environment.GetEnvironmentVariable("EafMigrator"));
            if (!skipConnVerification && !isDockerEnabled)
            {
                Log.Write("Continue to migration for database? (Y/N): ", false);
                var command = Console.ReadLine();
                if (!command.IsIn("Y", "y"))
                {
                    Log.Write("Migration canceled.");
                    return;
                }
            }

            Log.Write("Database migration started...");
            try
            {
                _migrator.CreateOrMigrateForHost(SeedHelper.SeedHostDb);
            }
            catch (Exception ex)
            {
                Log.Write("An error occured during database migration:");
                Log.Write(ex.ToString());
                Log.Write("Canceled migrations.");
                Console.ReadKey();
                throw;
            }

            Log.Write("Database migration completed.");
            Log.Write("--------------------------------------------------------");
        }
    }
}