using Abp.AutoMapper;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Modules;
using Abp.MultiTenancy;
using Abp.Reflection.Extensions;
using Eaf.Middleware;
using Eaf.ProjectName.EntityFrameworkCore;
using Eaf.ProjectName.Migrations.Seed;
using System;

namespace Eaf.ProjectName
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