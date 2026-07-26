using System;
using System.Threading.Tasks;
using GameHub.Catalog;
using GameHub.Developer;
using GameHub.Developer.Dto;
using GameHub.Gameplay;
using GameHub.Gameplay.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class GameMetricsAppService_Tests : GameHubTestBase
    {
        private readonly IGameMetricsAppService _gameMetricsAppService;
        private readonly IDeveloperGameAppService _developerGameAppService;

        public GameMetricsAppService_Tests()
        {
            _gameMetricsAppService = Resolve<IGameMetricsAppService>();
            _developerGameAppService = Resolve<IDeveloperGameAppService>();
        }

        [Fact]
        public async Task Dado_SessoesEEvents_Quando_ConsultarMetricas_Entao_RetornaTotaisEDiario()
        {
            var game = await _developerGameAppService.CreateDraftAsync(new CreateGameDraftInput
            {
                Title = "Metrics Game",
                ShortDescription = "Test game 01",
                AgeRating = "E",
                Orientation = "Both",
                SupportsDesktop = true,
                SupportsMobile = true,
                SupportsTablet = true
            });

            var sessionId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            await UsingDbContextAsync(async context =>
            {
                await context.PlaySessions.AddAsync(new PlaySession
                {
                    Id = sessionId,
                    GameId = game.Id,
                    TenantId = AbpSession.TenantId,
                    UserId = AbpSession.UserId,
                    StartedAt = now.AddMinutes(-5),
                    EndedAt = now,
                    DeviceType = "Desktop",
                    Browser = "TestBrowser",
                    CountryCode = "BR"
                });

                await context.GameplayEvents.AddRangeAsync(
                    new GameplayEvent
                    {
                        Id = Guid.NewGuid(),
                        PlaySessionId = sessionId,
                        GameId = game.Id,
                        TenantId = AbpSession.TenantId,
                        EventType = GameplayEventType.GameLoadingFinished,
                        OccurredAt = now.AddMinutes(-4)
                    },
                    new GameplayEvent
                    {
                        Id = Guid.NewGuid(),
                        PlaySessionId = sessionId,
                        GameId = game.Id,
                        TenantId = AbpSession.TenantId,
                        EventType = GameplayEventType.GameplayStarted,
                        OccurredAt = now.AddMinutes(-3)
                    },
                    new GameplayEvent
                    {
                        Id = Guid.NewGuid(),
                        PlaySessionId = sessionId,
                        GameId = game.Id,
                        TenantId = AbpSession.TenantId,
                        EventType = GameplayEventType.CommercialBreakCompleted,
                        OccurredAt = now.AddMinutes(-2)
                    });

                await context.SaveChangesAsync();
            });

            var result = await _gameMetricsAppService.GetMetricsAsync(game.Id, new GameMetricsFilter());

            result.ShouldNotBeNull();
            result.TotalPlays.ShouldBe(1);
            result.TotalUniquePlayers.ShouldBe(1);
            result.AverageDurationSeconds.ShouldBeGreaterThan(0);
            result.LoadingFinishedCount.ShouldBe(1);
            result.CommercialBreakCount.ShouldBe(1);
            result.Daily.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task Dado_FiltroPorPais_Quando_ConsultarMetricas_Entao_FiltraSessoes()
        {
            var game = await _developerGameAppService.CreateDraftAsync(new CreateGameDraftInput
            {
                Title = "Metrics Filter Game",
                ShortDescription = "Test game 01",
                AgeRating = "E",
                Orientation = "Both",
                SupportsDesktop = true,
                SupportsMobile = true,
                SupportsTablet = true
            });

            var now = DateTime.UtcNow;

            await UsingDbContextAsync(async context =>
            {
                await context.PlaySessions.AddRangeAsync(
                    new PlaySession
                    {
                        Id = Guid.NewGuid(),
                        GameId = game.Id,
                        TenantId = AbpSession.TenantId,
                        StartedAt = now,
                        DeviceType = "Desktop",
                        Browser = "TestBrowser",
                        CountryCode = "BR"
                    },
                    new PlaySession
                    {
                        Id = Guid.NewGuid(),
                        GameId = game.Id,
                        TenantId = AbpSession.TenantId,
                        StartedAt = now,
                        DeviceType = "Mobile",
                        Browser = "TestBrowser",
                        CountryCode = "US"
                    });

                await context.SaveChangesAsync();
            });

            var result = await _gameMetricsAppService.GetMetricsAsync(game.Id, new GameMetricsFilter { CountryCode = "BR" });

            result.ShouldNotBeNull();
            result.TotalPlays.ShouldBe(1);
        }

        [Fact]
        public async Task Dado_SessaoDePlaytest_Quando_ConsultarMetricas_Entao_NaoIncluiNaProducao()
        {
            var game = await _developerGameAppService.CreateDraftAsync(new CreateGameDraftInput
            {
                Title = "Metrics Playtest Game",
                ShortDescription = "Playtest filtering",
                AgeRating = "E",
                Orientation = "Both"
            });
            await UsingDbContextAsync(async context =>
            {
                await context.PlaySessions.AddRangeAsync(
                    new PlaySession
                    {
                        Id = Guid.NewGuid(),
                        GameId = game.Id,
                        TenantId = AbpSession.TenantId,
                        StartedAt = DateTime.UtcNow,
                        DeviceType = "Desktop",
                        Browser = "Test",
                        IsPlaytest = false
                    },
                    new PlaySession
                    {
                        Id = Guid.NewGuid(),
                        GameId = game.Id,
                        TenantId = AbpSession.TenantId,
                        StartedAt = DateTime.UtcNow,
                        DeviceType = "Desktop",
                        Browser = "Test",
                        IsPlaytest = true
                    });
                await context.SaveChangesAsync();
            });

            var result = await _gameMetricsAppService.GetMetricsAsync(
                game.Id,
                new GameMetricsFilter());

            result.TotalPlays.ShouldBe(1);
        }

        [Fact]
        public async Task Dado_MetricasValidas_Quando_ExportarCsv_Entao_RetornaCabecalhoEDados()
        {
            var game = await _developerGameAppService.CreateDraftAsync(new CreateGameDraftInput
            {
                Title = "Metrics Export Game",
                ShortDescription = "CSV export",
                AgeRating = "E",
                Orientation = "Both"
            });
            var sessionDate = DateTime.UtcNow.Date;

            await UsingDbContextAsync(async context =>
            {
                await context.PlaySessions.AddAsync(new PlaySession
                {
                    Id = Guid.NewGuid(),
                    GameId = game.Id,
                    TenantId = AbpSession.TenantId,
                    UserId = AbpSession.UserId,
                    StartedAt = sessionDate.AddHours(12),
                    DeviceType = "Desktop",
                    Browser = "Test"
                });
                await context.SaveChangesAsync();
            });

            var metrics = await _gameMetricsAppService.GetMetricsAsync(
                game.Id,
                new GameMetricsFilter
                {
                    From = sessionDate.AddDays(-1),
                    To = sessionDate.AddDays(1)
                });
            metrics.TotalPlays.ShouldBe(1);

            var export = await _gameMetricsAppService.ExportCsvAsync(
                game.Id,
                new GameMetricsFilter
                {
                    From = sessionDate.AddDays(-1),
                    To = sessionDate.AddDays(1)
                });

            export.FileName.ShouldBe("game-metrics.csv");
            export.ContentType.ShouldBe("text/csv");
            export.Content.ShouldContain("date,plays,uniquePlayers");
            export.Content.ShouldContain(",1,1,0");
        }
    }
}
