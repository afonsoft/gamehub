using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Playtesting;
using GameHub.Playtesting.Dto;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class PlaytestAnalyticsAppService_Tests : GameHubTestBase
    {
        private readonly IPlaytestAppService _playtestAppService;
        private readonly IRepository<PlaytestRecording, Guid> _recordingRepository;

        public PlaytestAnalyticsAppService_Tests()
        {
            _playtestAppService = LocalIocManager.Resolve<IPlaytestAppService>();
            _recordingRepository = LocalIocManager.Resolve<IRepository<PlaytestRecording, Guid>>();
        }

        [Fact]
        public async Task Dado_RecordingsComLevelEvents_Quando_DifficultyInsights_Entao_RetornaTaxaDeConclusao()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("Difficulty Game", "difficulty-game");
            var playtestId = await SeedPlaytestAsync(gameId);

            await UsingDbContextAsync(async context =>
            {
                await context.PlaytestRecordings.AddRangeAsync(
                    new PlaytestRecording
                    {
                        Id = Guid.NewGuid(),
                        TenantId = AbpSession.TenantId,
                        PlaytestSessionId = playtestId,
                        LevelEvents = "[{\"level\":\"1-1\",\"event\":\"start\"},{\"level\":\"1-1\",\"event\":\"death\"},{\"level\":\"1-1\",\"event\":\"restart\"},{\"level\":\"1-1\",\"event\":\"complete\"}]"
                    },
                    new PlaytestRecording
                    {
                        Id = Guid.NewGuid(),
                        TenantId = AbpSession.TenantId,
                        PlaytestSessionId = playtestId,
                        LevelEvents = "[{\"level\":\"1-1\",\"event\":\"start\"},{\"level\":\"1-1\",\"event\":\"death\"}]"
                    });

                await context.SaveChangesAsync();
            });

            var insights = await _playtestAppService.GetDifficultyInsightsAsync(gameId);

            insights.ShouldNotBeNull();
            insights.Levels.Count.ShouldBe(1);
            insights.Levels[0].Starts.ShouldBe(2);
            insights.Levels[0].Deaths.ShouldBe(2);
            insights.Levels[0].Completions.ShouldBe(1);
            insights.Levels[0].CompletionRate.ShouldBe(0.5, tolerance: 0.01);
        }

        private async Task<Guid> SeedGameAsync(string title, string slug)
        {
            var gameId = Guid.NewGuid();
            var profileId = Guid.NewGuid();
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

                await context.Games.AddAsync(new Game(gameId, title, slug, "Test", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Draft
                });
            });

            return gameId;
        }

        private async Task<Guid> SeedPlaytestAsync(Guid gameId)
        {
            var playtestId = Guid.NewGuid();
            await UsingDbContextAsync(async context =>
            {
                await context.PlaytestSessions.AddAsync(new PlaytestSession
                {
                    Id = playtestId,
                    TenantId = AbpSession.TenantId,
                    GameId = gameId,
                    RequestedByUserId = AbpSession.UserId ?? 1,
                    Status = PlaytestSessionStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                });
            });

            return playtestId;
        }
    }
}
