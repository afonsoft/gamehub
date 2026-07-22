using GameHub.Notifications;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Notifications
{
    public class GameHubNotificationProvider_Tests : GameHubTestBase
    {
        private readonly GameHubNotificationProvider _notificationProvider;

        public GameHubNotificationProvider_Tests()
        {
            _notificationProvider = LocalIocManager.Resolve<GameHubNotificationProvider>();
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
