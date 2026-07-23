using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.Mvc.Extensions;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.Extensions;
using Abp.PlugIns;
using Castle.Facilities.Logging;
using Eaf.AspNetCore.Configuration;
using Eaf.AspNetCore.Hangfire.Configuration;
using Eaf.AspNetCore.SignalR.Chat;
using Eaf.Castle.Logging.SerilogIntegration;
using Eaf.Middleware.Configuration;
using Eaf.Middleware.Swagger;
using Eaf.Middleware.Web.Authentication.JwtBearer;
using Eaf.Middleware.Web.Serilog;
using Eaf.Middleware.Web.Startup;
using Eaf.Middleware.Web.Swagger;
using GameHub.Application.Extensions;
using GameHub.Debugging;
using GameHub.Web.Configuration;
using GameHub.Web.WebHooks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace GameHub.Web.Startup
{
    public class Startup
    {
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public Startup(IWebHostEnvironment env)
        {
            _hostingEnvironment = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //MVC
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
                options.Filters.Add<SerilogMvcLoggingAttribute>();
                options.Filters.Add(new ResponseCacheAttribute() { NoStore = true, Location = ResponseCacheLocation.None });
            }).AddNewtonsoftJson();

            //Configure EAF Middleware
            services.AddEafConfigurer(_appConfiguration);

            //Configure HealthChecks
            services.AddEafHealthChecks();
            //Configure OpenTelemetry
            services.AddEafOpenTelemetry(options =>
            {
                options.ConsoleExporter = false;
                options.OtlpEndpoint = _appConfiguration["OpenTelemetry:OtlpEndpoint"]
                    ?? _appConfiguration["OTEL_EXPORTER_OTLP_ENDPOINT"]
                    ?? "https://otlp.nr-data.net:4318";
                options.OtlpVariables["OTEL_EXPORTER_OTLP_ENDPOINT"] = options.OtlpEndpoint;

                var otlpHeaders = _appConfiguration["OpenTelemetry:OtlpHeaders"]
                    ?? _appConfiguration["OTEL_EXPORTER_OTLP_HEADERS"];
                if (!string.IsNullOrEmpty(otlpHeaders))
                    options.OtlpVariables["OTEL_EXPORTER_OTLP_HEADERS"] = otlpHeaders;

                options.OtlpVariables["OTEL_EXPORTER_OTLP_PROTOCOL"] = _appConfiguration["OpenTelemetry:OtlpProtocol"]
                    ?? _appConfiguration["OTEL_EXPORTER_OTLP_PROTOCOL"]
                    ?? "http/protobuf";
                options.ServiceName = "GameHub";
                options.SourceName = new[]
                {
                    "GameHub.Web.Host",
                    "GameHub.EntityFrameworkCore",
                    "GameHub.Core",
                    "GameHub.Application"
                };
            });
            // Add OpenTelemetry and configure it to use Azure Monitor.

            // Configure CORS for GameHub Hub and Admin frontends
            services.AddGameHubCors(_appConfiguration);

            //Swagger - Enable this line and the related lines in Configure method to enable swagger UI
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "GameHub API",
                    Description = "GameHub",
                    Contact = new OpenApiContact
                    {
                        Name = "GameHub",
                        Email = "GameHub@afonsoft.com.br"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License"
                    }
                });

                options.DocInclusionPredicate((docName, description) => true);
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                options.CustomSchemaIds(type => type.FullName);
                options.IgnoreObsoleteActions();
                options.IgnoreObsoleteProperties();
                options.ParameterFilter<SwaggerEnumParameterFilter>();
                options.ParameterFilter<SwaggerNullableParameterFilter>();
                options.SchemaFilter<SwaggerEnumSchemaFilter>();
                options.OperationFilter<SwaggerOperationIdFilter>();
                options.OperationFilter<SwaggerOperationFilter>();
                options.CustomDefaultSchemaIdSelector();
                options.SupportNonNullableReferenceTypes();
            }).AddSwaggerGenNewtonsoftSupport();

            services.AddMemoryCache();

            // Response Compression (Brotli + Gzip)
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    "application/json",
                    "application/javascript",
                    "text/css",
                    "text/html",
                    "text/json",
                    "text/plain",
                    "text/xml"
                });
            });

            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            //Configure Eaf and Dependency Injection
            services.AddAbpWithoutCreatingServiceProvider<WebHostModule>(options =>
            {
                options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.UseEafSerilog()
                );
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            //Initializes Eaf framework.
            app.UseAbp(options =>
            {
                options.UseAbpRequestLocalization = false;
            });

            app.UseResponseCompression();
            app.UseEafHealthChecks();
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseJwtTokenMiddleware();
            app.UseAbpRequestLocalization();
            app.UseRouting();
            app.UseCors(GameHubConsts.DefaultCorsPolicyName);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<AbpCommonHub>("/signalr");
                endpoints.MapHub<ChatHub>("/signalr-chat");
                endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                //Map OpenTelemetry Metrics
                endpoints.MapEafOpenTelemetryMetrics();
                app.ApplicationServices.GetRequiredService<IAbpAspNetCoreConfiguration>().EndpointConfiguration.ConfigureAllEndpoints(endpoints);

                //Endpoint for receiver webhook, create any path and name
                endpoints.MapPost("/webhook", async (HttpContext context) =>
                {
                    var receiveWebHook = app.ApplicationServices.GetRequiredService<WebHookReceiver>();
                    receiveWebHook.context = context;
                    receiveWebHook.ReceiverName = "webhook";
                    using StreamReader stream = new(context.Request.Body);
                    await receiveWebHook.ProcessRequest(await stream.ReadToEndAsync());
                });
            });

            // Enable middleware HangFire
            // Storage type is resolved automatically by HangFireConfigurer.ResolveStorageType():
            // SQL Server provider -> SQL Server storage
            // Non-SQL Server + Redis enabled -> Redis storage
            // Non-SQL Server + Redis disabled -> InMemory storage
            var hangfireEnabled = _appConfiguration["Hangfire:IsEnabled"] != null && 
                                  bool.Parse(_appConfiguration["Hangfire:IsEnabled"]);
            
            if (_appConfiguration["Hangfire:IsEnabled"] != null)
            {
                app.UseEafHangfire(opt => 
                {
                    opt.IsEnabled = hangfireEnabled;
                    opt.StorageType = Eaf.Middleware.Web.Startup.HangFireConfigurer.ResolveStorageType(_appConfiguration);
                });
            }

            //For Security Only Swagger on Develop/Staging
            if (!_hostingEnvironment.IsProduction() || GameHubDebugHelper.IsDebug)
            {
                // Enable middleware to serve generated Swagger as a JSON endpoint
                app.UseSwagger();
                // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("../swagger/v1/swagger.json", "GameHub API V1");
                    options.IndexStream = () => Assembly.GetExecutingAssembly().GetManifestResourceStream("GameHub.Web.wwwroot.swagger.ui.index.html");
                    options.InjectBaseUrl(_appConfiguration["App:ServerRootAddress"]);
                });
            }

            //All Recurring Jobs
            if (hangfireEnabled)
                app.ScheduleRecurringJobs();
        }
    }
}