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
    public class ProjectNameCoreModule : AbpModule
    {
        public override void PreInitialize()
        {
            //Adding authorization providers
            Configuration.Authorization.Providers.Add<ProjectNameAuthorizationProvider>();

            //Adding setting providers
            Configuration.Settings.Providers.Add<ProjectNameSettingProvider>();

            //Adding notification providers
            Configuration.Notifications.Providers.Add<ProjectNameNotificationProvider>();

            //Adding feature providers
            Configuration.Features.Providers.Add<ProjectNameFeatureProvider>();

            //Starting localization settings
            ProjectNameLocalizationConfigurer.Configure(Configuration.Localization);

            //https://aspnetboilerplate.com/Pages/Documents/Multi-Tenancy
            //Enable this line to create a multi-tenant application.
            Configuration.MultiTenancy.IsEnabled = ProjectNameConsts.MultiTenancyEnabled;
            Configuration.MultiTenancy.IgnoreFeatureCheckForHostUsers = true;

            if (ProjectNameDebugHelper.IsDebug)
            {
                //Disabling email/sms sending in debug mode
                Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);
            }
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ProjectNameCoreModule).GetAssembly());
        }
    }
}