using Eaf.ProjectName.Notifications;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Core.Application.Notifications
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
