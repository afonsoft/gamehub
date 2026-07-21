using Abp;
using Abp.Domain.Services;
using Abp.Notifications;
using System.Threading.Tasks;

namespace Eaf.ProjectName.Notifications
{
    public class ProjectNameNotifier : DomainService, IProjectNameNotifier
    {
        private readonly INotificationPublisher _notificationPublisher;

        public ProjectNameNotifier(
            INotificationPublisher notificationPublisher
        )
        {
            _notificationPublisher = notificationPublisher;
        }

        public async Task SendMessageAsync(UserIdentifier user, string message, NotificationSeverity severity = NotificationSeverity.Info)
        {
            await _notificationPublisher.PublishAsync(
                ProjectNameNotificationNames.SimpleMessage,
                new MessageNotificationData(message),
                severity: severity,
                userIds: new[] { user }
                );
        }
    }
}