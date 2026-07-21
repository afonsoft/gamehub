using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Castle.MicroKernel.Registration;
using Castle.Windsor.MsDependencyInjection;
using Eaf.Middleware.Configuration;
using Eaf.Middleware.Identity;
using GameHub.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GameHub.Migrator
{
    [DependsOn(typeof(ProjectNameEntityFrameworkCoreModule))]
    public class MigratorModule : AbpModule
    {
        private readonly IConfigurationRoot _appConfiguration;

        public MigratorModule(ProjectNameEntityFrameworkCoreModule eafMiddlewareTemplateEntityFrameworkCoreModule)
        {
            eafMiddlewareTemplateEntityFrameworkCoreModule.SkipDbSeed = true;

            _appConfiguration = AppConfigurations.Get(
                typeof(MigratorModule).GetAssembly().GetDirectoryPathOrNull()
            );
        }

        public override void PreInitialize()
        {
            //ConnectionString Configurations
            var _environment = Environment.GetEnvironmentVariable("EafMigrator");
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(_environment);

            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
            Configuration.ReplaceService(typeof(IEventBus), () =>
            {
                IocManager.IocContainer.Register(
                    Component.For<IEventBus>().Instance(NullEventBus.Instance)
                );
            });
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(MigratorModule).GetAssembly());

            var services = new ServiceCollection();
            IdentityRegistrar.Register(services);
            WindsorRegistrationHelper.CreateServiceProvider(IocManager.IocContainer, services);
        }
    }
}