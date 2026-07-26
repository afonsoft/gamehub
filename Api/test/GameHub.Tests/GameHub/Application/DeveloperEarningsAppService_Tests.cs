using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub;
using GameHub.Catalog;
using GameHub.Developer;
using GameHub.Developer.Dto;
using GameHub.Developers;
using GameHub.Gameplay;
using GameHub.Monetization;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class DeveloperEarningsAppService_Tests : GameHubTestBase
    {
        private readonly IDeveloperEarningsAppService _developerEarningsAppService;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<DeveloperProfile, Guid> _profileRepository;

        public DeveloperEarningsAppService_Tests()
        {
            _developerEarningsAppService = LocalIocManager.Resolve<IDeveloperEarningsAppService>();
            _gameRepository = LocalIocManager.Resolve<IRepository<Game, Guid>>();
            _profileRepository = LocalIocManager.Resolve<IRepository<DeveloperProfile, Guid>>();
        }

        [Fact]
        public async Task Dado_JogoComBreaks_Quando_ConsultarEarnings_Entao_RetornaReceitaEstimada()
        {
            var gameId = await SeedGameWithProfileAsync();
            var contractService = LocalIocManager.Resolve<IRevenueContractAppService>();
            await contractService.SetContractAsync(gameId, RevenueContractType.WebExclusive);
            var today = DateTime.UtcNow.Date;

            await UsingDbContextAsync(async context =>
            {
                await context.GameMetricSnapshots.AddAsync(new GameMetricSnapshot
                {
                    Id = Guid.NewGuid(),
                    TenantId = AbpSession.TenantId,
                    GameId = gameId,
                    Date = today,
                    Plays = 100,
                    UniquePlayers = 80,
                    AvgDurationSeconds = 60,
                    LoadingFinishedCount = 90,
                    ErrorCount = 0,
                    CommercialBreakCount = 10,
                    RewardedBreakCount = 2
                });

                await context.SaveChangesAsync();
            });

            var result = await _developerEarningsAppService.GetEarningsAsync(new GetDeveloperEarningsInput());

            result.Games.Count.ShouldBe(1);
            result.TotalCommercialBreaks.ShouldBe(10);
            result.TotalRewardedBreaks.ShouldBe(2);
            result.TotalGrossEstimatedRevenue.ShouldBeGreaterThan(0);
            result.TotalDeveloperEstimatedRevenue.ShouldBeGreaterThan(0);
            result.Games[0].GrossEstimatedRevenue.ShouldBe(result.TotalGrossEstimatedRevenue);
        }

        [Fact]
        public async Task Dado_ContratoWebExclusive_Quando_ConsultarEarnings_Entao_DevShareMaiorQue50()
        {
            var gameId = await SeedGameWithProfileAsync();
            var contractService = LocalIocManager.Resolve<IRevenueContractAppService>();
            await contractService.SetContractAsync(gameId, RevenueContractType.WebExclusive);

            await UsingDbContextAsync(async context =>
            {
                await context.PlaySessions.AddAsync(new PlaySession
                {
                    Id = Guid.NewGuid(),
                    TenantId = AbpSession.TenantId,
                    GameId = gameId,
                    StartedAt = DateTime.UtcNow,
                    DeviceType = "Desktop",
                    Browser = "Test",
                    TrafficSource = TrafficSource.Platform
                });

                await context.GameMetricSnapshots.AddAsync(new GameMetricSnapshot
                {
                    Id = Guid.NewGuid(),
                    TenantId = AbpSession.TenantId,
                    GameId = gameId,
                    Date = DateTime.UtcNow.Date,
                    Plays = 1,
                    UniquePlayers = 1,
                    AvgDurationSeconds = 10,
                    LoadingFinishedCount = 1,
                    ErrorCount = 0,
                    CommercialBreakCount = 1,
                    RewardedBreakCount = 0
                });

                await context.SaveChangesAsync();
            });

            var result = await _developerEarningsAppService.GetEarningsAsync(new GetDeveloperEarningsInput());

            result.Games[0].DeveloperShare.ShouldBeGreaterThanOrEqualTo(0.5m);
            result.Games[0].ContractType.ShouldBe(RevenueContractType.WebExclusive);
        }

        private async Task<Guid> SeedGameWithProfileAsync()
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
                    DisplayName = "Earnings Tester",
                    Status = DeveloperProfileStatus.Active
                });

                await context.Games.AddAsync(new Game(gameId, "Earnings Game", "earnings-game", "Test game", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Published
                });

                await context.SaveChangesAsync();
            });

            return gameId;
        }
    }
}
