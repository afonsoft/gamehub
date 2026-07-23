using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Gameplay;
using GameHub.Jobs;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GameHub.Tests.GameHub.Jobs
{
    public class GameMetricsAggregationJob_Tests : GameHubTestBase
    {
        private readonly GameMetricsAggregationJob _job;

        public GameMetricsAggregationJob_Tests()
        {
            _job = Resolve<GameMetricsAggregationJob>();
        }

        [Fact]
        public async Task Dado_SessoesEEventos_Quando_Agregar_Entao_DeveCriarSnapshot()
        {
            var gameId = Guid.NewGuid();
            var profileId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var date = DateTime.UtcNow.Date;

            await UsingDbContextAsync(async context =>
            {
                await context.DeveloperProfiles.AddAsync(new DeveloperProfile
                {
                    Id = profileId,
                    TenantId = AbpSession.TenantId,
                    UserId = AbpSession.UserId ?? 1,
                    DisplayName = "Tester",
                    Status = DeveloperProfileStatus.Active
                });

                await context.Games.AddAsync(new Game(gameId, "Test Game", "test-game", "Test", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Published
                });

                await context.PlaySessions.AddAsync(new PlaySession
                {
                    Id = sessionId,
                    GameId = gameId,
                    StartedAt = date.AddHours(1),
                    EndedAt = date.AddHours(1).AddMinutes(5),
                    DeviceType = "Desktop",
                    Browser = "Chrome",
                    UserId = AbpSession.UserId,
                    TenantId = AbpSession.TenantId,
                });

                await context.GameplayEvents.AddAsync(new GameplayEvent
                {
                    Id = Guid.NewGuid(),
                    PlaySessionId = sessionId,
                    GameId = gameId,
                    EventType = GameplayEventType.GameLoadingFinished,
                    OccurredAt = date.AddHours(1).AddSeconds(10),
                    TenantId = AbpSession.TenantId,
                });

                await context.GameplayEvents.AddAsync(new GameplayEvent
                {
                    Id = Guid.NewGuid(),
                    PlaySessionId = sessionId,
                    GameId = gameId,
                    EventType = GameplayEventType.CommercialBreakCompleted,
                    OccurredAt = date.AddHours(1).AddSeconds(20),
                    TenantId = AbpSession.TenantId,
                });
            });

            await _job.Execute(new GameMetricsAggregationArgs { Date = date });

            await UsingDbContextAsync(async context =>
            {
                var snapshots = await context.GameMetricSnapshots
                    .Where(s => s.GameId == gameId)
                    .ToListAsync();

                var snapshot = snapshots.FirstOrDefault(s => s.Date == date);

                snapshot.ShouldNotBeNull();
                snapshot.Plays.ShouldBe(1);
                snapshot.UniquePlayers.ShouldBe(1);
                snapshot.LoadingFinishedCount.ShouldBe(1);
                snapshot.CommercialBreakCount.ShouldBe(1);
                snapshot.AvgDurationSeconds.ShouldBe(300, tolerance: 0.1);
            });
        }

        [Fact]
        public async Task Dado_SnapshotExistente_Quando_AgregarNovamente_Entao_DeveAtualizar()
        {
            var gameId = Guid.NewGuid();
            var profileId = Guid.NewGuid();
            var date = DateTime.UtcNow.Date;

            await UsingDbContextAsync(async context =>
            {
                await context.DeveloperProfiles.AddAsync(new DeveloperProfile
                {
                    Id = profileId,
                    TenantId = AbpSession.TenantId,
                    UserId = AbpSession.UserId ?? 1,
                    DisplayName = "Tester 2",
                    Status = DeveloperProfileStatus.Active
                });

                await context.Games.AddAsync(new Game(gameId, "Test Game 2", "test-game-2", "Test", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Published
                });

                await context.GameMetricSnapshots.AddAsync(new GameMetricSnapshot
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    Date = date,
                    Plays = 0,
                    UniquePlayers = 0,
                    AvgDurationSeconds = 0,
                    LoadingFinishedCount = 0,
                    ErrorCount = 0,
                    CommercialBreakCount = 0,
                    RewardedBreakCount = 0,
                    TenantId = AbpSession.TenantId,
                });
            });

            await _job.Execute(new GameMetricsAggregationArgs { Date = date });

            await UsingDbContextAsync(async context =>
            {
                var snapshots = await context.GameMetricSnapshots
                    .Where(s => s.GameId == gameId)
                    .ToListAsync();

                var snapshot = snapshots.FirstOrDefault(s => s.Date == date);

                snapshot.ShouldNotBeNull();
            });
        }
    }
}
