using Eaf.Middleware.Notifications;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Eaf.ProjectName.Tests.Notifications
{
    // ReSharper disable once InconsistentNaming
    public class NotificationAppService_Tests : ProjectNameTestBase
    {
        private readonly INotificationAppService _notificationAppService;

        public NotificationAppService_Tests()
        {
            _notificationAppService = Resolve<INotificationAppService>();
        }

        [Fact]
        public async Task Test_ChangeNotificationSettings()
        {
            var settings = await _notificationAppService.GetNotificationSettings();
            settings.ReceiveNotifications.ShouldBe(true);
            settings.Notifications.Count.ShouldBeGreaterThan(0);
        }
    }
}