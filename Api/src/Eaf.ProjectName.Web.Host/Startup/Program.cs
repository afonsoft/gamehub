using Abp.AspNetCore.Dependency;
using Abp.Dependency;
using Eaf.Configuration;
using Eaf.Middleware.Configuration;
using Eaf.Middleware.Web.Serilog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Eaf.ProjectName.Web.Startup
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("pt-BR");
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                await CreateHostBuilder(args).Build().RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal Error in Main : {0}", ex.Message);
                Environment.Exit(1);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
                .UseCastleWindsor(IocManager.Instance.IocContainer)
#if DEBUG
                .UseEafSerilog(Serilog.Events.LogEventLevel.Debug)
#else
                .UseEafSerilog(Serilog.Events.LogEventLevel.Information)
#endif
                .UseEafConfiguration(prefix: "ProjectName_")
                .UseEafKeyVault(opt => opt.Provider = KeyVault.EnumKeyVault.None)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                    webBuilder.UseKestrel(opt =>
                    {
                        opt.AddServerHeader = false;
                        opt.Limits.MaxRequestLineSize = 16 * 1024;
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}