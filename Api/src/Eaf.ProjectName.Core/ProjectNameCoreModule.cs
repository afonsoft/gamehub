using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.Reflection.Extensions;
using Eaf.Middleware;
using Eaf.ProjectName.Authorization;
using Eaf.ProjectName.Configuration;
using Eaf.ProjectName.Debugging;
using Eaf.ProjectName.Features;
using Eaf.ProjectName.Localization;
using Eaf.ProjectName.Notifications;

namespace Eaf.ProjectName
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