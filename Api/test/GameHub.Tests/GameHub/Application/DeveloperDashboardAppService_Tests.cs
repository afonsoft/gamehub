using System;
using System.Threading.Tasks;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Developer;
using GameHub.Developer.Dto;
using GameHub.Gameplay;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class DeveloperDashboardAppService_Tests : GameHubTestBase
    {
        private readonly IDeveloperDashboardAppService _dashboardAppService;
        private readonly IDeveloperGameAppService _developerGameAppService;

        public DeveloperDashboardAppService_Tests()
        {
            _dashboardAppService = Resolve<IDeveloperDashboardAppService>();
            _developerGameAppService = Resolve<IDeveloperGameAppService>();
        }

        [Fact]
        public async Task Dado_PerfilDeDesenvolvedorSemJogos_Quando_CarregarDashboard_Entao_RetornaVazio()
        {
            var result = await _dashboardAppService.GetDashboardAsync();

            result.ShouldNotBeNull();
            result.TotalGames.ShouldBe(0);
            result.RecentVersions.ShouldBeEmpty();
            result.PlaysOverTime.ShouldNotBeNull();
        }

        [Fact]
        public async Task Dado_JogosEBuidsDoDesenvolvedor_Quando_CarregarDashboard_Entao_RetornaResumo()
        {
            var game1 = await _developerGameAppService.CreateDraftAsync(new CreateGameDraftInput
            {
                Title = "Dashboard Game 1",
                ShortDescription = "Test game 01",
                AgeRating = "E",
                Orientation = "Both",
                SupportsDesktop = true,
                SupportsMobile = true,
                SupportsTablet = true
            });

            var game2 = await _developerGameAppService.CreateDraftAsync(new CreateGameDraftInput
            {
                Title = "Dashboard Game 2",
                ShortDescription = "Test game 01",
                AgeRating = "E",
                Orientation = "Both",
                SupportsDesktop = true,
                SupportsMobile = true,
                SupportsTablet = true
            });

            await UsingDbContextAsync(async context =>
            {
                var g1 = await context.Games.FirstAsync(g => g.Id == game1.Id);
                g1.Status = GameStatus.InReview;

                var g2 = await context.Games.FirstAsync(g => g.Id == game2.Id);
                g2.Status = GameStatus.Draft;

                var build = new GameBuild(
                    Guid.NewGuid(),
                    game2.Id,
                    "1.0.1",
                    1,
                    "https://example.com/build.zip",
                    1024,
                    "sha256hash")
                {
                    TenantId = AbpSession.TenantId
                };
                await context.GameBuilds.AddAsync(build);

                await context.SaveChangesAsync();
            });

            var result = await _dashboardAppService.GetDashboardAsync();

            result.ShouldNotBeNull();
            result.TotalGames.ShouldBe(2);
            result.PendingReviewGames.ShouldBe(1);
            result.DraftGames.ShouldBe(1);
            result.PendingActions.Count.ShouldBe(2);
            result.RecentVersions.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task Dado_JogosComMetricas_Quando_CarregarDashboard_Entao_RetornaPlaysOverTime()
        {
            var game = await _developerGameAppService.CreateDraftAsync(new CreateGameDraftInput
            {
                Title = "Metric Game",
                ShortDescription = "Test game 01",
                AgeRating = "E",
                Orientation = "Both",
                SupportsDesktop = true,
                SupportsMobile = true,
                SupportsTablet = true
            });

            await UsingDbContextAsync(async context =>
            {
                await context.GameMetricSnapshots.AddAsync(new GameMetricSnapshot
                {
                    Id = Guid.NewGuid(),
                    GameId = game.Id,
                    TenantId = AbpSession.TenantId,
                    Date = DateTime.Now.AddDays(-1).Date,
                    Plays = 42,
                    UniquePlayers = 30,
                    AvgDurationSeconds = 120,
                    LoadingFinishedCount = 40,
                    ErrorCount = 2,
                    CommercialBreakCount = 0,
                    RewardedBreakCount = 0
                });
                await context.SaveChangesAsync();
            });

            var result = await _dashboardAppService.GetDashboardAsync();

            result.ShouldNotBeNull();
            result.PlaysOverTime.Count.ShouldBe(7);
            result.PlaysOverTime.ShouldContain(p => p.Plays == 42);
        }
    }
}
