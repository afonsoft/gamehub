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
using Eaf.Middleware.Web.Authentication;
using GameHub.EntityFrameworkCore;
using GameHub.Security;
using GameHub.Storage;
using GameHub.Tests.DependencyInjection;
using GameHub.Multiplayer;
using GameHub.Web.Multiplayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Data.Common;
using System.Threading.Tasks;
using Abp.MultiTenancy;
using GameHub.Tests.MultiTenancy;

namespace GameHub.Tests
{
    [DependsOn(
        typeof(GameHubApplicationModule),
        typeof(GameHubCoreModule),
        typeof(GameHubEntityFrameworkCoreModule),
        typeof(AbpTestBaseModule)
        )]
    public class GameHubTestModule : AbpModule
    {
        public GameHubTestModule(GameHubEntityFrameworkCoreModule EafGameHubEntityFrameworkModule)
        {
            EafGameHubEntityFrameworkModule.SkipDbContextRegistration = true;
            EafGameHubEntityFrameworkModule.SkipDbSeed = true;
        }

        public override void PreInitialize()
        {
            GameHubDbContext.SkipMigrate = true;

            Configuration.UnitOfWork.Timeout = TimeSpan.FromMinutes(30);
            Configuration.UnitOfWork.IsTransactional = false;

            // Disable static mapper usage since it breaks unit tests (see https://github.com/aspnetboilerplate/aspnetboilerplate/issues/2052)
#pragma warning disable CS0618 // IAbpAutoMapperConfiguration.UseStaticMapper is obsolete and has no replacement in ABP 10.4
            Configuration.Modules.AbpAutoMapper().UseStaticMapper = false;
#pragma warning restore CS0618

            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;

            //Use database for language management
            Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

            //https://aspnetboilerplate.com/Pages/Documents/Timing
            Clock.Provider = ClockProviders.Utc;

            RegisterFakeService<AbpZeroDbMigrator<GameHubDbContext>>();
            IocManager.RegisterIfNot<IGameAssetStorage, FakeGameAssetStorage>(DependencyLifeStyle.Transient);
            IocManager.RegisterIfNot<IGameTokenProvider, FakeGameTokenProvider>(DependencyLifeStyle.Transient);
            IocManager.RegisterIfNot<ITokenAuthenticationService, FakeTokenAuthenticationService>(DependencyLifeStyle.Transient);
            IocManager.RegisterIfNot<IMultiplayerPresenceStore, CacheMultiplayerPresenceStore>(
                DependencyLifeStyle.Transient);

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
