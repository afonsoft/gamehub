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

            var result = await provider.ShowCommercialBreakAsync(Guid.NewGuid());

            result.ShouldNotBeNull();
            result.Completed.ShouldBeTrue();
            result.AdBlocked.ShouldBeFalse();
        }

        [Fact]
        public async Task Dado_RewardedBreak_Quando_Executar_Entao_DeveConcederRecompensa()
        {
            var provider = new FakeAdProvider();

            var result = await provider.ShowRewardedBreakAsync(Guid.NewGuid());

            result.ShouldNotBeNull();
            result.Completed.ShouldBeTrue();
            result.RewardGranted.ShouldBeTrue();
        }

        [Fact]
        public async Task Dado_AmbienteBloqueado_Quando_ExecutarBreakComercial_Entao_DeveRetornarBloqueado()
        {
            var provider = new FakeAdProvider { SimulateAdBlocked = true };

            var result = await provider.ShowCommercialBreakAsync(Guid.NewGuid());

            result.ShouldNotBeNull();
            result.Completed.ShouldBeFalse();
            result.AdBlocked.ShouldBeTrue();
        }

        [Fact]
        public async Task Dado_AmbienteBloqueado_Quando_ExecutarRewarded_Entao_NaoDeveConcederRecompensa()
        {
            var provider = new FakeAdProvider { SimulateAdBlocked = true };

            var result = await provider.ShowRewardedBreakAsync(Guid.NewGuid());

            result.ShouldNotBeNull();
            result.Completed.ShouldBeFalse();
            result.RewardGranted.ShouldBeFalse();
        }
    }
}
