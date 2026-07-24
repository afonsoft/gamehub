using System;
using System.Threading.Tasks;
using GameHub.Builds;
using GameHub.Builds.Dto;
using GameHub.Catalog;
using GameHub.Developers;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class GamePreviewAppService_Tests : GameHubTestBase
    {
        private readonly IGamePreviewAppService _gamePreviewAppService;

        public GamePreviewAppService_Tests()
        {
            _gamePreviewAppService = Resolve<IGamePreviewAppService>();
        }

        [Fact]
        public async Task Dado_BuildExistente_Quando_CriarPreviewToken_Entao_DeveRetornarTokenEUrl()
        {
            var (gameId, buildId) = await CriarJogoEBuildAsync();

            var result = await _gamePreviewAppService.CreatePreviewTokenAsync(new CreatePreviewTokenInput
            {
                GameId = gameId,
                Version = "1.0.0"
            });

            result.ShouldNotBeNull();
            result.Token.ShouldNotBeNullOrWhiteSpace();
            result.PreviewUrl.ShouldContain("/preview/preview-test/1.0.0?token=");
        }

        [Fact]
        public async Task Dado_TokenValido_Quando_Validar_Entao_DeveRetornarUrlDePreview()
        {
            var (gameId, buildId) = await CriarJogoEBuildAsync();

            var tokenResult = await _gamePreviewAppService.CreatePreviewTokenAsync(new CreatePreviewTokenInput
            {
                GameId = gameId,
                Version = "1.0.0"
            });

            var validation = await _gamePreviewAppService.ValidatePreviewAsync(new ValidatePreviewInput
            {
                Token = tokenResult.Token
            });

            validation.ShouldNotBeNull();
            validation.IsValid.ShouldBeTrue();
            validation.PreviewUrl.ShouldContain("https://cdn.test/build/index.html");
        }

        [Fact]
        public async Task Dado_TokenInvalido_Quando_Validar_Entao_DeveRetornarErro()
        {
            var validation = await _gamePreviewAppService.ValidatePreviewAsync(new ValidatePreviewInput
            {
                Token = "invalid-token"
            });

            validation.ShouldNotBeNull();
            validation.IsValid.ShouldBeFalse();
            validation.Error.ShouldNotBeNullOrWhiteSpace();
        }

        private async Task<(Guid gameId, Guid buildId)> CriarJogoEBuildAsync()
        {
            var gameId = Guid.NewGuid();
            var buildId = Guid.NewGuid();
            var profileId = Guid.NewGuid();
            var userId = AbpSession.UserId.Value;

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

                await context.Games.AddAsync(new Game(gameId, "Preview Test", "preview-test", "Test", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Published,
                    ThumbnailUrl = "https://cdn.test/thumb.png"
                });

                await context.GameBuilds.AddAsync(new GameBuild
                {
                    Id = buildId,
                    TenantId = AbpSession.TenantId,
                    GameId = gameId,
                    Version = "1.0.0",
                    BuildNumber = 1,
                    Status = GameBuildStatus.Published,
                    OriginalPackageUrl = "https://cdn.test/build.zip",
                    PublicBaseUrl = "https://cdn.test/build/",
                    IndexHtmlPath = "index.html",
                    SizeBytes = 1024,
                    HashSha256 = "abc"
                });
            });

            return (gameId, buildId);
        }
    }
}
