using System.Linq;
using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.Mvc.Extensions;
using Eaf.SignalR.Hubs;
using Abp.AspNetCore.TestBase;
using Abp.Dependency;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Eaf.AspNetCore.SignalR.Chat;
using Eaf.Castle.Logging.SerilogIntegration;
using Eaf.Middleware.Configuration;
using Eaf.Middleware.Web.Authentication.JwtBearer;
using Eaf.Middleware.Web.Serilog;
using Eaf.Middleware.Web.Startup;
using GameHub.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace GameHub.Web.Tests
{
    public class Startup
    {
        private readonly IConfigurationRoot _appConfiguration;

        public Startup(IWebHostEnvironment env)
        {
            _appConfiguration = env.GetAppConfiguration();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFrameworkInMemoryDatabase();
            _appConfiguration["Hangfire:IsInMemoryDatabase"] = "true";

            //MVC
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
                options.Filters.Add<SerilogMvcLoggingAttribute>();
                options.Filters.Add(new ResponseCacheAttribute() { NoStore = true, Location = ResponseCacheLocation.None });
            }).AddNewtonsoftJson();

            services.AddEafConfigurer(_appConfiguration);

            //Configure Eaf and Dependency Injection
            return services.AddAbp<GameHubWebTestModule>(options =>
            {
                if (!options.IocManager.IocContainer.Kernel.GetFacilities().OfType<LoggingFacility>().Any())
                {
                    options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                        f => f.UseEafSerilog()
                    );
                }

                options.SetupTest();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            UseInMemoryDb(app.ApplicationServices);

            //Initializes Eaf framework.
            app.UseAbp(options =>
            {
                options.UseAbpRequestLocalization = false;
            });

            app.UseJwtTokenMiddleware();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
             {
                 endpoints.MapHub<EafCommonHub>("/signalr");
                 endpoints.MapHub<ChatHub>("/signalr-chat");

                 endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
                 endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                 app.ApplicationServices.GetRequiredService<IAbpAspNetCoreConfiguration>().EndpointConfiguration.ConfigureAllEndpoints(endpoints);
             });
        }

        private void UseInMemoryDb(IServiceProvider serviceProvider)
        {
            var builder = new DbContextOptionsBuilder<GameHubDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).UseInternalServiceProvider(serviceProvider);
            var options = builder.Options;

            var iocManager = serviceProvider.GetRequiredService<IIocManager>();
            if (!iocManager.IocContainer.Kernel.HasComponent(typeof(DbContextOptions<GameHubDbContext>)))
            {
                iocManager.IocContainer
                    .Register(
                        Component.For<DbContextOptions<GameHubDbContext>>()
                            .Instance(options)
                            .LifestyleSingleton()
                    );
            }
        }
    }
}