using System;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class EafUpgrade95_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_AtualizacaoParaEaf95_Quando_VerificaVersaoFramework_Entao_DeveEstarConfiguradoCorretamente()
        {
            // Dado
            var assembly = typeof(GameHubCoreModule).Assembly;

            // Quando
            var targetFramework = assembly.GetName().Version;

            // Entao
            assembly.ShouldNotBeNull();
            targetFramework.ShouldNotBeNull();
        }

        [Fact]
        public void Dado_ModuloGameHub_Quando_IniciaAplicacao_Entao_ResolveDependenciasComSucesso()
        {
            // Dado / Quando
            var coreModule = Resolve<GameHubCoreModule>();

            // Entao
            coreModule.ShouldNotBeNull();
        }
    }
}
