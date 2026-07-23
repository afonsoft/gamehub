using Abp.AspNetCore.Configuration;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Castle.MicroKernel.Registration;
using Eaf.KeyVault.AspNetCore;
using Eaf.Middleware.Configuration;
using Eaf.Middleware.Web;
using GameHub.Catalog;
using GameHub.Gameplay;
using GameHub.Storage;
using GameHub.Web.Caching;
using GameHub.Web.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;

namespace GameHub.Web.Startup
{
    [DependsOn(
        typeof(GameHubApplicationModule),
        typeof(MiddlewareWebCoreModule),
        typeof(EafKeyVaultAspNetCoreModule)
    )]
    public class WebHostModule : AbpModule
    {
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public WebHostModule(
            IWebHostEnvironment env
        )
        {
            _hostingEnvironment = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(WebHostModule).GetAssembly());

            //Enabled or Disabled BackgroundJobs
            Configuration.BackgroundJobs.IsJobExecutionEnabled = true;
        }

        public override void PreInitialize()
        {
            //Set default connection string
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(GameHubConsts.ConnectionStringName);

            // Bind storage options and register MinIO/S3 implementation
            var storageOptions = new StorageOptions();
            _appConfiguration.GetSection("Storage").Bind(storageOptions);
            IocManager.IocContainer.Register(Component.For<StorageOptions>().Instance(storageOptions));
            IocManager.Register<IGameAssetStorage, MinioGameAssetStorage>(DependencyLifeStyle.Transient);

            // Replace in-memory caches with Redis implementations when Redis is enabled
            var redisCacheEnabled = bool.TryParse(_appConfiguration["RedisCache:IsEnabled"], out var redisEnabled) && redisEnabled;
            var redisConnectionString = _appConfiguration["RedisCache:ConnectionString"];
            if (redisCacheEnabled && !string.IsNullOrWhiteSpace(redisConnectionString))
            {
                IocManager.IocContainer.Register(
                    Component.For<IConnectionMultiplexer>()
                        .UsingFactoryMethod(() => ConnectionMultiplexer.Connect(redisConnectionString))
                        .LifestyleSingleton());

                Configuration.ReplaceService<IGameCatalogCache, RedisGameCatalogCache>(DependencyLifeStyle.Transient);
                Configuration.ReplaceService<ILeaderboardCache, RedisLeaderboardCache>(DependencyLifeStyle.Transient);
            }

            //Create Controllers APIs
            Configuration.Modules.AbpAspNetCore()
                .CreateControllersForAppServices(
                    typeof(GameHubApplicationModule).GetAssembly()
                );

            //Send All Exceptions To Clients Angular only in develop/staging
            if (!_hostingEnvironment.IsProduction())
                Configuration.Modules.AbpWebCommon().SendAllExceptionsToClients = true;
            else
                Configuration.Modules.AbpWebCommon().SendAllExceptionsToClients = false;

            //Enable Delete Expired Logs
            Configuration.EntityHistory.IsEnabled = true;
            Configuration.Auditing.IsEnabled = true;

            Configuration.Caching.MemoryCacheOptions = new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions
            {
                SizeLimit = 256 //Mb
            };
        }

    }
}
