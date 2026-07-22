using Abp.AutoMapper;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Modules;
using Abp.MultiTenancy;
using Abp.Reflection.Extensions;
using Eaf.Middleware;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.EntityFrameworkCore;
using GameHub.Gameplay;
using GameHub.Migrations.Seed;
using GameHub.Monetization;
using System;

namespace GameHub
{
    [DependsOn(
        typeof(ProjectNameEntityFrameworkCoreModule),
        typeof(MiddlewareApplicationModule)
    )]
    public class ProjectNameApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            //Adding custom AutoMapper configuration
            Configuration.Modules.AbpAutoMapper().Configurators.Add(ProjectNameCustomDtoMapper.CreateMappings);

            IocManager.Register<IGameCatalogCache, InMemoryGameCatalogCache>(DependencyLifeStyle.Transient);
            IocManager.Register<ILeaderboardCache, InMemoryLeaderboardCache>(DependencyLifeStyle.Transient);
            IocManager.Register<IGameBuildPackageValidator, GameBuildPackageValidator>(DependencyLifeStyle.Transient);
            IocManager.Register<IAdProvider, FakeAdProvider>(DependencyLifeStyle.Transient);
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ProjectNameApplicationModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            using (var _connectionStringResolver = IocManager.ResolveAsDisposable<DefaultConnectionStringResolver>())
            {
                var hostConnStr = _connectionStringResolver.Object.GetNameOrConnectionString(new ConnectionStringResolveArgs(MultiTenancySides.Host));
                if (hostConnStr.IsNullOrWhiteSpace())
                {
                    Logger.Error("Configuration file should contain a connection string");
                    return;
                }

                using (var _startupConfiguration = IocManager.ResolveAsDisposable<IAbpStartupConfiguration>())
                {
                    using (var _migrator = IocManager.ResolveAsDisposable<ProjectNameDbMigrator>())
                    {
                        Logger.Info("Database migration started...");
                        try
                        {
                            _migrator.Object.CreateOrMigrateForHost(SeedHelper.SeedHostDb);
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat(ex, "An error occured during hangfire migration: {0}", ex.Message);
                            Logger.Info("Canceled migrations.");
                            return;
                        }

                        Logger.Info("Database migration completed.");
                    }
                }
            }
        }
    }
}