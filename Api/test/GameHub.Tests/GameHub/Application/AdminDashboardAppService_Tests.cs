using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub.Admin;
using GameHub.Admin.Dto;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Gameplay;
using GameHub.Moderation;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class AdminDashboardAppService_Tests : GameHubTestBase
    {
        private readonly IAdminDashboardAppService _adminDashboardAppService;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<GameBuild, Guid> _buildRepository;
        private readonly IRepository<ModerationReview, Guid> _reviewRepository;
        private readonly IRepository<PlaySession, Guid> _playSessionRepository;
        private readonly IRepository<GameplayEvent, Guid> _gameplayEventRepository;

        public AdminDashboardAppService_Tests()
        {
            _adminDashboardAppService = LocalIocManager.Resolve<IAdminDashboardAppService>();
            _gameRepository = LocalIocManager.Resolve<IRepository<Game, Guid>>();
            _buildRepository = LocalIocManager.Resolve<IRepository<GameBuild, Guid>>();
            _reviewRepository = LocalIocManager.Resolve<IRepository<ModerationReview, Guid>>();
            _playSessionRepository = LocalIocManager.Resolve<IRepository<PlaySession, Guid>>();
            _gameplayEventRepository = LocalIocManager.Resolve<IRepository<GameplayEvent, Guid>>();
        }

        [Fact]
        public async Task Dado_DadosNoSistema_Quando_Resumo_Entao_RetornaContadoresCorretos()
        {
            await SeedDashboardDataAsync();

            var result = await _adminDashboardAppService.GetSummaryAsync();

            result.ShouldNotBeNull();
            result.TotalGames.ShouldBeGreaterThanOrEqualTo(1);
            result.TotalBuilds.ShouldBeGreaterThanOrEqualTo(1);
            result.PendingUploads.ShouldBeGreaterThanOrEqualTo(1);
            result.PendingReviews.ShouldBeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task Dado_PlaySessionsRecentes_Quando_SerieTemporal_Entao_RetornaUltimosDias()
        {
            var gameId = Guid.NewGuid();
            await SeedGameAsync(gameId, "Plays Chart Game");
            await SeedPlaySessionAsync(gameId, DateTime.UtcNow.AddDays(-1));

            var result = await _adminDashboardAppService.GetPlaysOverTimeAsync(7);

            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(7);
            result.Items.Sum(i => i.Plays).ShouldBeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task Dado_BuildsRecentes_Quando_AtividadesRecentes_Entao_RetornaListas()
        {
            await SeedDashboardDataAsync();

            var uploads = await _adminDashboardAppService.GetRecentUploadsAsync(5);
            uploads.Items.Count.ShouldBeGreaterThanOrEqualTo(1);

            var games = await _adminDashboardAppService.GetRecentGamesAsync(5);
            games.Items.Count.ShouldBeGreaterThanOrEqualTo(1);

            var topGames = await _adminDashboardAppService.GetTopGamesAsync(5);
            topGames.Items.Count.ShouldBeGreaterThanOrEqualTo(1);

            var reviews = await _adminDashboardAppService.GetPendingReviewsAsync(5);
            reviews.Items.Count.ShouldBeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task Dado_EventosEPlays_Quando_Metricas_Entao_RetornaAgregadosEAlertas()
        {
            var gameId = Guid.NewGuid();
            await SeedGameAsync(gameId, "Metrics Game");
            await SeedPlaySessionAsync(gameId, DateTime.UtcNow, DateTime.UtcNow.AddSeconds(45), "Desktop", "BR", "Chrome");
            await SeedGameplayEventAsync(gameId, GameplayEventType.GameLoadingStarted, DateTime.UtcNow);
            await SeedGameplayEventAsync(gameId, GameplayEventType.GameLoadingStarted, DateTime.UtcNow);
            await SeedGameplayEventAsync(gameId, GameplayEventType.GameLoadingStarted, DateTime.UtcNow);
            await SeedGameplayEventAsync(gameId, GameplayEventType.GameLoadingFinished, DateTime.UtcNow);
            await SeedGameplayEventAsync(gameId, GameplayEventType.GameplayStarted, DateTime.UtcNow);
            await SeedGameplayEventAsync(gameId, GameplayEventType.GameErrorCaptured, DateTime.UtcNow);

            var metrics = await _adminDashboardAppService.GetMetricsAsync(null, null);

            metrics.ShouldNotBeNull();
            metrics.TotalPlays.ShouldBe(1);
            metrics.AverageSessionDurationSeconds.ShouldBeGreaterThan(40);
            metrics.Devices.Count.ShouldBe(1);
            metrics.Countries.Count.ShouldBe(1);
            metrics.Browsers.Count.ShouldBe(1);
            metrics.LoadConversionRate.ShouldBe(0.33, 0.02);

            var alerts = await _adminDashboardAppService.GetHealthAlertsAsync();
            alerts.ShouldContain(a => a.GameId == gameId && a.Reason == "Load conversion below 50%");
        }

        private async Task SeedDashboardDataAsync()
        {
            var gameId = Guid.NewGuid();
            var buildId = Guid.NewGuid();
            var reviewId = Guid.NewGuid();

            await SeedGameAsync(gameId, "Dashboard Game");

            await UsingDbContextAsync(async context =>
            {
                await context.GameBuilds.AddAsync(new GameBuild(buildId, gameId, "1.0.1", 1, "/uploads/test.zip", 1024, "hash")
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameBuildStatus.Uploaded
                });

                await context.ModerationReviews.AddAsync(new ModerationReview
                {
                    Id = reviewId,
                    TenantId = AbpSession.TenantId,
                    GameId = gameId,
                    GameBuildId = buildId,
                    Status = ModerationReviewStatus.Pending
                });

                await context.SaveChangesAsync();
            });
        }

        private async Task SeedGameAsync(Guid gameId, string title)
        {
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

                await context.Games.AddAsync(new Game(gameId, title, title.ToLowerInvariant().Replace(" ", "-"), "Test game", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Draft,
                    TotalPlays = 5
                });

                await context.SaveChangesAsync();
            });
        }

        private async Task SeedPlaySessionAsync(Guid gameId, DateTime startedAt, DateTime? endedAt = null, string deviceType = "Desktop", string countryCode = "BR", string browser = "TestBrowser")
        {
            await UsingDbContextAsync(async context =>
            {
                await context.PlaySessions.AddAsync(new PlaySession
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    TenantId = AbpSession.TenantId,
                    UserId = AbpSession.UserId,
                    StartedAt = startedAt,
                    EndedAt = endedAt,
                    DeviceType = deviceType,
                    Browser = browser,
                    CountryCode = countryCode
                });

                await context.SaveChangesAsync();
            });
        }

        private async Task SeedGameplayEventAsync(Guid gameId, GameplayEventType eventType, DateTime occurredAt)
        {
            await UsingDbContextAsync(async context =>
            {
                await context.GameplayEvents.AddAsync(new GameplayEvent
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    TenantId = AbpSession.TenantId,
                    EventType = eventType,
                    OccurredAt = occurredAt
                });

                await context.SaveChangesAsync();
            });
        }
    }
}
