using GameHub.Debugging;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core.Application
{
    public class GameHubDebugHelper_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_GameHubDebugHelper_Quando_VerificarIsDebug_Entao_DeveRetornarBoolean()
        {
            var isDebug = GameHubDebugHelper.IsDebug;
            isDebug.ShouldBeOfType<bool>();
        }
    }
}
