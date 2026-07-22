using Abp;
using Abp.Domain.Services;
using Abp.Notifications;
using System.Threading.Tasks;

namespace GameHub.Notifications
{
    public class GameHubNotifier : DomainService, IGameHubNotifier
    {
        private readonly INotificationPublisher _notificationPublisher;

        public GameHubNotifier(
            INotificationPublisher notificationPublisher
        )
        {
            _notificationPublisher = notificationPublisher;
        }

        public async Task SendMessageAsync(UserIdentifier user, string message, NotificationSeverity severity = NotificationSeverity.Info)
        {
            await _notificationPublisher.PublishAsync(
                GameHubNotificationNames.SimpleMessage,
                new MessageNotificationData(message),
                severity: severity,
                userIds: new[] { user }
                );
        }
    }
}