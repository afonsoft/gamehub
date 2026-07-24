using System;
using System.IO;
using System.Threading.Tasks;
using GameHub.Admin;
using GameHub.Admin.Dto;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Storage;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class AdminGameAppService_Tests : GameHubTestBase
    {
        private readonly IAdminGameAppService _adminGameAppService;
        private readonly IGameBuildAppService _gameBuildAppService;

        public AdminGameAppService_Tests()
        {
            _adminGameAppService = Resolve<IAdminGameAppService>();
            _gameBuildAppService = Resolve<IGameBuildAppService>();
        }

        [Fact]
        public async Task Dado_BuildComRequestsExternosSemPolitica_Quando_Publicar_Entao_DeveLancarExcecao()
        {
            var (gameId, buildId) = await CriarJogoEBuildComExternalAsync();

            await Should.ThrowAsync<InvalidOperationException>(async () =>
                await _adminGameAppService.PublishAsync(new PublishGameInput
                {
                    GameId = gameId,
                    GameBuildId = buildId
                }));
        }

        [Fact]
        public async Task Dado_BuildComRequestsExternosComPolitica_Quando_Publicar_Entao_DevePublicar()
        {
            var (gameId, buildId) = await CriarJogoEBuildComExternalAsync(true);

            await _adminGameAppService.PublishAsync(new PublishGameInput
            {
                GameId = gameId,
                GameBuildId = buildId
            });

            await UsingDbContextAsync(async context =>
            {
                var game = await context.Games.FindAsync(gameId);
                game.Status.ShouldBe(GameStatus.Published);
            });
        }

        private async Task<(Guid gameId, Guid buildId)> CriarJogoEBuildComExternalAsync(bool withPrivacyPolicy = false)
        {
            var userId = AbpSession.UserId.Value;
            var gameId = Guid.NewGuid();
            var profileId = Guid.NewGuid();
            var buildId = Guid.NewGuid();

            await UsingDbContextAsync(async context =>
            {
                await context.DeveloperProfiles.AddAsync(new DeveloperProfile
                {
                    Id = profileId,
                    TenantId = AbpSession.TenantId,
                    UserId = userId,
                    DisplayName = "Dev User",
                    Status = DeveloperProfileStatus.Active
                });

                await context.Games.AddAsync(new Game(gameId, "Privacy Test", "privacy-test", "Test", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.InReview,
                    PrivacyPolicyUrl = withPrivacyPolicy ? "https://example.com/privacy" : null
                });

                await context.GameBuilds.AddAsync(new GameBuild(buildId, gameId, "1.0.0", 1, "http://pkg", 1024, "hash")
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameBuildStatus.Approved
                });

                await context.BuildValidationReports.AddAsync(new BuildValidationReport
                {
                    Id = Guid.NewGuid(),
                    TenantId = AbpSession.TenantId,
                    GameBuildId = buildId,
                    IsValid = true,
                    HasExternalRequests = true,
                    CreatedAt = DateTime.UtcNow
                });
            });

            return (gameId, buildId);
        }
    }
}
