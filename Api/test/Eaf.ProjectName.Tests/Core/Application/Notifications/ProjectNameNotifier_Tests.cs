using Eaf.ProjectName.Notifications;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Core.Application.Notifications
{
    public class ProjectNameNotifier_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_ProjectNameNotifier_Quando_CriarInstancia_Entao_DeveSerValido()
        {
            var notifier = LocalIocManager.Resolve<IProjectNameNotifier>();
            notifier.ShouldNotBeNull();
        }
    }
}
