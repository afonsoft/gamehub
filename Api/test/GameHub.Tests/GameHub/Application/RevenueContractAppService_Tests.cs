using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Monetization;
using GameHub.Monetization.Dto;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class RevenueContractAppService_Tests : GameHubTestBase
    {
        private readonly IRevenueContractAppService _revenueContractAppService;
        private readonly IRepository<Game, Guid> _gameRepository;

        public RevenueContractAppService_Tests()
        {
            _revenueContractAppService = LocalIocManager.Resolve<IRevenueContractAppService>();
            _gameRepository = LocalIocManager.Resolve<IRepository<Game, Guid>>();
        }

        [Fact]
        public async Task Dado_JogoSemContrato_Quando_CalcularShare_Entao_UsaNonExclusive()
        {
            var gameId = await SeedGameAsync("Direct Game", "direct-game");

            var result = await _revenueContractAppService.CalculateShareAsync(gameId, TrafficSource.Direct);

            result.ContractType.ShouldBe(RevenueContractType.NonExclusive);
            result.DeveloperShare.ShouldBe(1m);
            result.PlatformShare.ShouldBe(0m);
        }

        [Fact]
        public async Task Dado_ContratoNonExclusive_Quando_TrafegoPlataforma_Entao_Split50_50()
        {
            var gameId = await SeedGameAsync("Platform Game", "platform-game");
            await _revenueContractAppService.SetContractAsync(gameId, RevenueContractType.NonExclusive);

            var result = await _revenueContractAppService.CalculateShareAsync(gameId, TrafficSource.Platform);

            result.DeveloperShare.ShouldBe(0.5m);
            result.PlatformShare.ShouldBe(0.5m);
        }

        [Fact]
        public async Task Dado_ContratoWebExclusive_Quando_TrafegoPlataforma_Entao_DevRecebe70()
        {
            var gameId = await SeedGameAsync("Exclusive Game", "exclusive-game");
            await _revenueContractAppService.SetContractAsync(gameId, RevenueContractType.WebExclusive);

            var result = await _revenueContractAppService.CalculateShareAsync(gameId, TrafficSource.Homepage);

            result.ContractType.ShouldBe(RevenueContractType.WebExclusive);
            result.DeveloperShare.ShouldBe(0.7m);
            result.PlatformShare.ShouldBe(0.3m);
        }

        [Fact]
        public async Task Dado_ContratoSetado_Quando_GetByGame_Entao_RetornaContratoAtivo()
        {
            var gameId = await SeedGameAsync("Active Contract Game", "active-contract-game");
            await _revenueContractAppService.SetContractAsync(gameId, RevenueContractType.WebExclusive);

            var contract = await _revenueContractAppService.GetByGameAsync(gameId);

            contract.ShouldNotBeNull();
            contract.GameId.ShouldBe(gameId);
            contract.ContractType.ShouldBe(RevenueContractType.WebExclusive);
            contract.IsActive.ShouldBeTrue();
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
                    Status = GameStatus.InReview
                });

                await context.SaveChangesAsync();
            });

            return gameId;
        }
    }
}
