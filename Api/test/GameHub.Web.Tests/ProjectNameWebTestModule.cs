using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.TestBase;
using Abp.AutoMapper;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.Reflection.Extensions;
using Abp.Timing;
using Abp.Zero.Configuration;
using Abp.Zero.EntityFrameworkCore;
using Castle.Core.Logging;
using Castle.MicroKernel.Registration;
using Eaf.KeyVault.AspNetCore;
using Eaf.Middleware.Web;
using Eaf.Middleware.Web.Authentication.JwtBearer;
using GameHub.EntityFrameworkCore;
using GameHub.Tests;
using GameHub.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Net;

namespace GameHub.Web.Tests
{
    [DependsOn(
        typeof(WebHostModule),
        typeof(ProjectNameTestModule),
        typeof(ProjectNameApplicationModule),
        typeof(ProjectNameCoreModule),
        typeof(ProjectNameEntityFrameworkCoreModule),
        typeof(AbpAspNetCoreTestBaseModule),
        typeof(MiddlewareWebCoreModule),
        typeof(EafKeyVaultAspNetCoreModule),
        typeof(ProjectNameTestModule)
    )]
    public class ProjectNameWebTestModule : AbpModule
    {
        public ProjectNameWebTestModule(ProjectNameEntityFrameworkCoreModule EafProjectNameEntityFrameworkModule)
        {
            EafProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
            EafProjectNameEntityFrameworkModule.SkipDbSeed = true;
        }

        public override void PreInitialize()
        {
            ProjectNameDbContext.SkipMigrate = true;

            IocManager.RegisterIfNot<TokenAuthConfiguration>();
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.

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
            //Create Controllers APIs
            Configuration.Modules.AbpAspNetCore()
                .CreateControllersForAppServices(
                    typeof(ProjectNameApplicationModule).GetAssembly()
                );

            Configuration.Modules.AbpWebCommon().SendAllExceptionsToClients = true;

            //Enable Delete Expired Logs
            Configuration.EntityHistory.IsEnabled = true;
            Configuration.Auditing.IsEnabled = true;

            Configuration.Caching.MemoryCacheOptions = new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions
            {
                SizeLimit = 256 //Mb
            };
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ProjectNameWebTestModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            IocManager.RegisterIfNot<IEmailSender, NullEmailSender>();

            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(ProjectNameWebTestModule).Assembly);

            Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);

            IocManager.RegisterIfNot<IEmailSender, NullEmailSender>();
            IocManager.RegisterIfNot(typeof(ILogger<>), typeof(Logger<>));
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
