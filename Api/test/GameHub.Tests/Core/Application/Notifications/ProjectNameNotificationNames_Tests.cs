using GameHub.Notifications;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core.Application.Notifications
{
    public class ProjectNameNotificationNames_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_ProjectNameNotificationNames_Quando_VerificarConsts_Entao_DeveTerValoresValidos()
        {
            ProjectNameNotificationNames.SimpleMessage.ShouldNotBeNullOrEmpty();
        }
    }
}
