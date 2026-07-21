using Abp;
using Abp.Notifications;
using System.Threading.Tasks;

namespace Eaf.ProjectName.Notifications
{
    public interface IProjectNameNotifier
    {
        Task SendMessageAsync(UserIdentifier user, string message, NotificationSeverity severity = NotificationSeverity.Info);
    }
}