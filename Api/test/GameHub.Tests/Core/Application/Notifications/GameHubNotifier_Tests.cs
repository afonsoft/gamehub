using GameHub.Notifications;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core.Application.Notifications
{
    public class GameHubNotifier_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_GameHubNotifier_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            var notifier = LocalIocManager.Resolve<IGameHubNotifier>();
            notifier.ShouldNotBeNull();
        }
    }
}
