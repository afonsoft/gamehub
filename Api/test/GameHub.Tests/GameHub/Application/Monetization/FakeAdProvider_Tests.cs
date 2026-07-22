using System;
using System.Threading.Tasks;
using GameHub.Monetization;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application.Monetization
{
    public class FakeAdProvider_Tests
    {
        [Fact]
        public async Task Dado_BreakComercial_Quando_Executar_Entao_DeveConcluirSemErro()
        {
            var provider = new FakeAdProvider();

            await provider.ShowCommercialBreakAsync(Guid.NewGuid());

            // Null object: não levanta exceção.
            true.ShouldBeTrue();
        }

        [Fact]
        public async Task Dado_RewardedBreak_Quando_Executar_Entao_DeveRetornarConcluido()
        {
            var provider = new FakeAdProvider();

            var completed = await provider.ShowRewardedBreakAsync(Guid.NewGuid());

            completed.ShouldBeTrue();
        }
    }
}
