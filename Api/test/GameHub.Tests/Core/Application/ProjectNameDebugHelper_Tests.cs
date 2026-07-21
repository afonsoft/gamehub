using GameHub.Debugging;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core.Application
{
    public class ProjectNameDebugHelper_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_ProjectNameDebugHelper_Quando_VerificarIsDebug_Entao_DeveRetornarBoolean()
        {
            var isDebug = ProjectNameDebugHelper.IsDebug;
            isDebug.ShouldBeOfType<bool>();
        }
    }
}
