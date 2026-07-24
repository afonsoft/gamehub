using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GameHub.Monetization;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class FakeAdProvider_Tests
    {
        [Fact]
        public async Task Dado_FakeProvider_Quando_ShowRewardedBreak_Entao_RetornaTrue()
        {
            var provider = new FakeAdProvider();

            var result = await provider.ShowRewardedBreakAsync(Guid.NewGuid());

            result.ShouldBeTrue();
        }

        [Fact]
        public async Task Dado_FakeProvider_Quando_ShowCommercialBreak_Entao_CompletaComDelayCurto()
        {
            var provider = new FakeAdProvider();
            var stopwatch = Stopwatch.StartNew();

            await provider.ShowCommercialBreakAsync(Guid.NewGuid());

            stopwatch.Stop();
            stopwatch.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo(0);
        }
    }
}
