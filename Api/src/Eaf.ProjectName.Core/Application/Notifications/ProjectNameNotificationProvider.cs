using Abp.Authorization;
using Abp.Localization;
using Abp.Notifications;
using Eaf.Middleware.Authorization;

namespace Eaf.ProjectName.Notifications
{
    public class ProjectNameNotificationProvider : NotificationProvider
    {
        public override void SetNotifications(INotificationDefinitionContext context)
        {
            context.Manager.Add(
                new NotificationDefinition(
                    ProjectNameNotificationNames.SimpleMessage,
                    displayName: L("TestSendMensage"),
                    permissionDependency: new SimplePermissionDependency(MiddlewarePermissions.Pages_Administration_Users)
                    )
                );
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, ProjectNameConsts.LocalizationSourceName);
        }
    }
}