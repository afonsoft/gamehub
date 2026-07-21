using System;
using System.Linq;
using System.Threading.Tasks;
using GameHub.Gameplay;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class LeaderboardCache_Tests
    {
        [Fact]
        public async Task Dado_MultiplasPontuacoes_Quando_ConsultarTop_Entao_DeveOrdenarMaiorPrimeiro()
        {
            var cache = new InMemoryLeaderboardCache();
            var gameId = Guid.NewGuid();

            await cache.SubmitScoreAsync(gameId, 1, 100);
            await cache.SubmitScoreAsync(gameId, 2, 500);
            await cache.SubmitScoreAsync(gameId, 3, 300);

            var top = await cache.GetTopAsync(gameId, 10);

            top[0].Score.ShouldBe(500);
            top[0].Rank.ShouldBe(1);
            top[1].Score.ShouldBe(300);
            top[2].Score.ShouldBe(100);
        }

        [Fact]
        public async Task Dado_TopN_Quando_Consultar_Entao_DeveRetornarApenasN()
        {
            var cache = new InMemoryLeaderboardCache();
            var gameId = Guid.NewGuid();

            for (var i = 1; i <= 5; i++)
            {
                await cache.SubmitScoreAsync(gameId, i, i * 10);
            }

            var top = await cache.GetTopAsync(gameId, 3);

            top.Count.ShouldBe(3);
        }
    }
}
