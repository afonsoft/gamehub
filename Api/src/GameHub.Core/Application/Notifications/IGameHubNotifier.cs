using Abp;
using Abp.Notifications;
using System.Threading.Tasks;

namespace GameHub.Notifications
{
    public interface IGameHubNotifier
    {
        Task SendMessageAsync(UserIdentifier user, string message, NotificationSeverity severity = NotificationSeverity.Info);
    }
}