using GameHub.Notifications;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Notifications
{
    public class ProjectNameNotificationProvider_Tests : ProjectNameTestBase
    {
        private readonly ProjectNameNotificationProvider _notificationProvider;

        public ProjectNameNotificationProvider_Tests()
        {
            _notificationProvider = LocalIocManager.Resolve<ProjectNameNotificationProvider>();
        }

        [Fact]
        public void Dado_ProviderInicializado_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            // Dado (Given) & Quando (When)
            var provider = _notificationProvider;
            
            // Então (Then)
            provider.ShouldNotBeNull();
        }
    }
}
