using GameHub.Debugging;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Debugging
{
    public class GameHubDebugHelper_Tests
    {
        [Fact]
        public void Dado_DebugHelper_Quando_VerificarIsDebug_Entao_DeveRetornarBooleano()
        {
            var isDebug = GameHubDebugHelper.IsDebug;
            isDebug.ShouldBeOneOf(true, false);
        }

        [Fact]
        public void Dado_DebugHelper_Quando_VerificarTipo_Entao_DeveSerClasseEstatica()
        {
            var type = typeof(GameHubDebugHelper);
            type.IsAbstract.ShouldBeTrue();
            type.IsSealed.ShouldBeTrue();
        }

#if DEBUG
        [Fact]
        public void Dado_ModoDebug_Quando_VerificarIsDebug_Entao_DeveSerTrue()
        {
            GameHubDebugHelper.IsDebug.ShouldBeTrue();
        }
#endif
    }
}
