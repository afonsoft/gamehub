using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.Reflection.Extensions;
using Eaf.Middleware;
using GameHub.Authorization;
using GameHub.Configuration;
using GameHub.Debugging;
using GameHub.Features;
using GameHub.Localization;
using GameHub.Notifications;

namespace GameHub
{
    [DependsOn(
        typeof(MiddlewareCoreModule))
    ]
    public class GameHubCoreModule : AbpModule
    {
        public override void PreInitialize()
        {
            //Adding authorization providers
            Configuration.Authorization.Providers.Add<GameHubAuthorizationProvider>();

            //Adding setting providers
            Configuration.Settings.Providers.Add<GameHubSettingProvider>();

            //Adding notification providers
            Configuration.Notifications.Providers.Add<GameHubNotificationProvider>();

            //Adding feature providers
            Configuration.Features.Providers.Add<GameHubFeatureProvider>();

            //Starting localization settings
            GameHubLocalizationConfigurer.Configure(Configuration.Localization);

            //https://aspnetboilerplate.com/Pages/Documents/Multi-Tenancy
            //Enable this line to create a multi-tenant application.
            Configuration.MultiTenancy.IsEnabled = GameHubConsts.MultiTenancyEnabled;
            Configuration.MultiTenancy.IgnoreFeatureCheckForHostUsers = true;

            if (GameHubDebugHelper.IsDebug)
            {
                //Disabling email/sms sending in debug mode
                Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);
            }
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(GameHubCoreModule).GetAssembly());
        }
    }
}