using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub.Catalog;
using GameHub.Developer;
using GameHub.Developer.Dto;
using GameHub.Developers;
using GameHub.Monetization;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class DeveloperEarningsAdReportAppService_Tests : GameHubTestBase
    {
        private readonly IDeveloperEarningsAppService _earningsAppService;

        public DeveloperEarningsAdReportAppService_Tests()
        {
            _earningsAppService = LocalIocManager.Resolve<IDeveloperEarningsAppService>();
        }

        [Fact]
        public async Task Dado_ImpressoesRegistradas_Quando_RelatorioDeAnuncios_Entao_AgregaPorTipoEFornecedor()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("Ad Report Game", "ad-report-game");

            await UsingDbContextAsync(async context =>
            {
                await context.AdImpressions.AddRangeAsync(
                    new AdImpression
                    {
                        Id = Guid.NewGuid(),
                        TenantId = AbpSession.TenantId,
                        GameId = gameId,
                        Type = "commercial",
                        Provider = "Fake",
                        CountryCode = "BR",
                        DeviceType = "Desktop",
                        Earnings = 0.002m,
                        OccurredAt = DateTime.UtcNow
                    },
                    new AdImpression
                    {
                        Id = Guid.NewGuid(),
                        TenantId = AbpSession.TenantId,
                        GameId = gameId,
                        Type = "rewarded",
                        Provider = "Fake",
                        CountryCode = "US",
                        DeviceType = "Mobile",
                        Earnings = 0.01m,
                        OccurredAt = DateTime.UtcNow
                    });

                await context.SaveChangesAsync();
            });

            var report = await _earningsAppService.GetAdReportAsync(new GetDeveloperEarningsInput());

            report.ShouldNotBeNull();
            report.TotalImpressions.ShouldBe(2);
            report.TotalEarnings.ShouldBe(0.012m, tolerance: 0.0001m);
            report.Items.Count.ShouldBe(2);
        }

        private async Task<Guid> SeedGameAsync(string title, string slug)
        {
            var gameId = Guid.NewGuid();
            var profileId = Guid.NewGuid();
            await UsingDbContextAsync(async context =>
            {
                var userId = AbpSession.UserId ?? 1;
                await context.DeveloperProfiles.AddAsync(new DeveloperProfile
                {
                    Id = profileId,
                    TenantId = AbpSession.TenantId,
                    UserId = userId,
                    DisplayName = "Tester",
                    Status = DeveloperProfileStatus.Active
                });

                await context.Games.AddAsync(new Game(gameId, title, slug, "Test", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    DeveloperProfileId = profileId,
                    Status = GameStatus.Draft
                });
            });

            return gameId;
        }
    }
}
