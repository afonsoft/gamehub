using System;
using System.Threading.Tasks;
using Abp.Runtime.Session;
using GameHub.Web.Caching;
using NSubstitute;
using Shouldly;
using StackExchange.Redis;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class RedisLeaderboardCache_Tests
    {
        [Fact]
        public async Task Dado_Pontuacao_Quando_Submeter_Entao_IncrementaSortedSet()
        {
            // Arrange
            var multiplexer = Substitute.For<IConnectionMultiplexer>();
            var database = Substitute.For<IDatabase>();
            multiplexer.GetDatabase().Returns(database);

            var cache = new RedisLeaderboardCache(multiplexer, NullAbpSession.Instance);
            var gameId = Guid.NewGuid();

            // Act
            await cache.SubmitScoreAsync(gameId, 1, 100);

            // Assert
            await database.Received(1).SortedSetIncrementAsync(
                Arg.Any<RedisKey>(),
                "1",
                100,
                Arg.Any<CommandFlags>());
        }

        [Fact]
        public async Task Dado_Pontuacoes_Quando_ConsultarTop_Entao_RetornaRankOrdenado()
        {
            // Arrange
            var multiplexer = Substitute.For<IConnectionMultiplexer>();
            var database = Substitute.For<IDatabase>();
            multiplexer.GetDatabase().Returns(database);

            database.SortedSetRangeByRankWithScoresAsync(
                    Arg.Any<RedisKey>(),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Order.Descending,
                    Arg.Any<CommandFlags>())
                .Returns(Task.FromResult(new SortedSetEntry[]
                {
                    new SortedSetEntry("2", 500),
                    new SortedSetEntry("1", 300)
                }));

            var cache = new RedisLeaderboardCache(multiplexer, NullAbpSession.Instance);
            var gameId = Guid.NewGuid();

            // Act
            var top = await cache.GetTopAsync(gameId, 3);

            // Assert
            top.Count.ShouldBe(2);
            top[0].Rank.ShouldBe(1);
            top[0].UserId.ShouldBe(2);
            top[0].Score.ShouldBe(500);
            top[1].Rank.ShouldBe(2);
            top[1].UserId.ShouldBe(1);
            top[1].Score.ShouldBe(300);
        }
    }
}
