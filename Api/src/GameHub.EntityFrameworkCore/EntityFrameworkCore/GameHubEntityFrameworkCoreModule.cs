using Abp;
using Abp.Dependency;
using Abp.EntityFrameworkCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.EntityFrameworkCore;
using GameHub.EntityHistory;
using GameHub.Migrations.Seed;
using Microsoft.Extensions.Logging;

namespace GameHub.EntityFrameworkCore
{
    [DependsOn(
        typeof(AbpZeroCoreEntityFrameworkCoreModule),
        typeof(GameHubCoreModule)
    )]
    public class GameHubEntityFrameworkCoreModule : AbpModule
    {
        public bool SkipDbContextRegistration { get; set; }

        public bool SkipDbSeed { get; set; }

        public override void PreInitialize()
        {
            if (!SkipDbContextRegistration)
            {
                Configuration.Modules.AbpEfCore().AddDbContext<GameHubDbContext>(options =>
                {
                    options.DbContextOptions.EnableDetailedErrors(true);
                    options.DbContextOptions.EnableSensitiveDataLogging(false);

                    if (Configuration.IocManager.IsRegistered<ILoggerFactory>())
                    {
                        options.DbContextOptions.UseLoggerFactory(Configuration.IocManager.Resolve<ILoggerFactory>());
                    }

                    if (options.ExistingConnection != null)
                        GameHubDbContextConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
                    else
                        GameHubDbContextConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
                });
            }

            Configuration.EntityHistory.Selectors.Add("GameHubEntities", EntityHistoryHelper.TrackedTypes);
            Configuration.CustomConfigProviders.Add(new EntityHistoryConfigProvider(Configuration));
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(GameHubEntityFrameworkCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            using (var scope = IocManager.CreateScope())
            {
                if (!SkipDbSeed)
                {
                    SeedHelper.SeedHostDb(IocManager);
                }
            }
        }
    }
}