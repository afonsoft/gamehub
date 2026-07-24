using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Gameplay;
using GameHub.Monetization;
using GameHub.Monetization.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class AdBreakAppService_Tests : GameHubTestBase
    {
        private readonly IAdBreakAppService _adBreakAppService;
        private readonly IRepository<Game, Guid> _gameRepository;

        public AdBreakAppService_Tests()
        {
            _adBreakAppService = LocalIocManager.Resolve<IAdBreakAppService>();
            _gameRepository = LocalIocManager.Resolve<IRepository<Game, Guid>>();
        }

        [Fact]
        public async Task Dado_BreakComercial_Quando_Solicitar_Entao_RetornaConcluido()
        {
            var gameId = await SeedGameAsync("Ad Game", "ad-game");

            var result = await _adBreakAppService.RequestCommercialBreakAsync(new RequestAdBreakInput { GameId = gameId });

            result.ShouldNotBeNull();
            result.Completed.ShouldBeTrue();
        }

        [Fact]
        public async Task Dado_RewardedBreak_Quando_Solicitar_Entao_RetornaConcluidoERecompensa()
        {
            var gameId = await SeedGameAsync("Rewarded Game", "rewarded-game");

            var result = await _adBreakAppService.RequestRewardedBreakAsync(new RequestAdBreakInput { GameId = gameId });

            result.ShouldNotBeNull();
            result.Completed.ShouldBeTrue();
            result.RewardGranted.ShouldBeTrue();
            result.AdBlocked.ShouldBeFalse();
        }

        [Fact]
        public async Task Dado_SessaoExistente_Quando_BreakComercialConcluido_Entao_IncrementaMetricas()
        {
            var gameId = await SeedGameAsync("Metric Game", "metric-game");
            var sessionId = await SeedPlaySessionAsync(gameId);

            var result = await _adBreakAppService.RequestCommercialBreakAsync(new RequestAdBreakInput
            {
                GameId = gameId,
                SessionId = sessionId
            });

            result.Completed.ShouldBeTrue();

            await UsingDbContextAsync(async context =>
            {
                var session = await context.PlaySessions.FindAsync(sessionId);
                session.ShouldNotBeNull();
                session.CommercialBreakCount.ShouldBe(1);

                var metric = await context.GameMetricSnapshots.FirstOrDefaultAsync(m => m.GameId == gameId);
                metric.ShouldNotBeNull();
                metric.CommercialBreakCount.ShouldBe(1);
            });
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

                await context.Games.AddAsync(new Game(gameId, title, slug, "Test game", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Published
                });

                await context.SaveChangesAsync();
            });

            return gameId;
        }

        private async Task<Guid> SeedPlaySessionAsync(Guid gameId)
        {
            var sessionId = Guid.NewGuid();

            await UsingDbContextAsync(async context =>
            {
                await context.PlaySessions.AddAsync(new PlaySession
                {
                    Id = sessionId,
                    TenantId = AbpSession.TenantId,
                    GameId = gameId,
                    UserId = AbpSession.UserId,
                    StartedAt = DateTime.UtcNow,
                    DeviceType = "Desktop",
                    Browser = "Test",
                    TrafficSource = TrafficSource.Direct
                });

                await context.SaveChangesAsync();
            });

            return sessionId;
        }
    }
}
