using Abp.AutoMapper;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.TestBase;
using Abp.Timing;
using Abp.Zero.Configuration;
using Abp.Zero.EntityFrameworkCore;
using Abp.EntityFrameworkCore;
using Castle.MicroKernel.Registration;
using Eaf.ProjectName.EntityFrameworkCore;
using Eaf.ProjectName.Tests.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Data.Common;
using System.Threading.Tasks;
using Abp.MultiTenancy;

namespace Eaf.ProjectName.Tests
{
    [DependsOn(
        typeof(ProjectNameApplicationModule),
        typeof(ProjectNameCoreModule),
        typeof(ProjectNameEntityFrameworkCoreModule),
        typeof(AbpTestBaseModule)
        )]
    public class ProjectNameTestModule : AbpModule
    {
        public ProjectNameTestModule(ProjectNameEntityFrameworkCoreModule EafProjectNameEntityFrameworkModule)
        {
            EafProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
            EafProjectNameEntityFrameworkModule.SkipDbSeed = true;
        }

        public override void PreInitialize()
        {
            ProjectNameDbContext.SkipMigrate = true;

            Configuration.UnitOfWork.Timeout = TimeSpan.FromMinutes(30);
            Configuration.UnitOfWork.IsTransactional = false;

            // Disable static mapper usage since it breaks unit tests (see https://github.com/aspnetboilerplate/aspnetboilerplate/issues/2052)
            Configuration.Modules.AbpAutoMapper().UseStaticMapper = false;

            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;

            //Use database for language management
            Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

            //https://aspnetboilerplate.com/Pages/Documents/Timing
            Clock.Provider = ClockProviders.Utc;

            RegisterFakeService<AbpZeroDbMigrator<ProjectNameDbContext>>();

            Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);
        }

        public override void PostInitialize()
        {
            IocManager.RegisterIfNot<IEmailSender, NullEmailSender>();
            IocManager.RegisterIfNot(typeof(ILogger<>), typeof(Logger<>));
            IocManager.RegisterIfNot<global::Castle.Core.Logging.ILogger, global::Castle.Core.Logging.NullLogger>();
        }

        public override void Initialize()
        {
            ServiceCollectionRegistrar.Register(IocManager);
        }

        private void RegisterFakeService<TService>() where TService : class
        {
            IocManager.IocContainer.Register(
                Component.For<TService>()
                    .UsingFactoryMethod(() => Substitute.For<TService>())
                    .LifestyleSingleton()
            );
        }
    }
}
