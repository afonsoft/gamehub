using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Castle.MicroKernel.Registration;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Storage;
using NSubstitute;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class GameBuildAppService_Tests : GameHubTestBase
    {
        private readonly IGameBuildAppService _gameBuildAppService;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<GameBuild, Guid> _buildRepository;

        public GameBuildAppService_Tests()
        {
            _gameBuildAppService = LocalIocManager.Resolve<IGameBuildAppService>();
            _gameRepository = LocalIocManager.Resolve<IRepository<Game, Guid>>();
            _buildRepository = LocalIocManager.Resolve<IRepository<GameBuild, Guid>>();
        }

        [Fact]
        public async Task Dado_PackageValido_Quando_UploadBuild_Entao_PersistePublicBaseUrlEIndexHtmlPath()
        {
            var gameId = await SeedGameAsync();
            var stream = CreateZipStream();

            var result = await _gameBuildAppService.UploadBuildAsync(gameId, stream, "build.zip", "application/zip");

            result.Status.ShouldBe(GameBuildStatus.Validated.ToString());
            result.BuildId.ShouldNotBe(Guid.Empty);
            result.Version.ShouldMatch("1.0.*");
            result.ValidationSummary.ShouldNotBeNull();
            result.ValidationSummary.IsValid.ShouldBeTrue();
            result.ValidationSummary.HasIndexHtml.ShouldBeTrue();

            var build = await _buildRepository.GetAsync(result.BuildId);
            build.OriginalPackageUrl.ShouldBe($"http://minio/gamehub/builds/{gameId:N}/{result.BuildId:N}/build.zip");
            build.PublicBaseUrl.ShouldBe($"http://minio/gamehub/builds/{gameId:N}/{result.BuildId:N}/");
            build.IndexHtmlPath.ShouldBe("index.html");
            build.Status.ShouldBe(GameBuildStatus.Validated);
        }

        [Fact]
        public async Task Dado_PackageSemIndexHtml_Quando_UploadBuild_Entao_RetornaValidationFailed()
        {
            var gameId = await SeedGameAsync();
            using var stream = CreateZipWithoutIndex();

            var result = await _gameBuildAppService.UploadBuildAsync(gameId, stream, "build.zip", "application/zip");

            result.Status.ShouldBe(GameBuildStatus.ValidationFailed.ToString());
            result.BuildId.ShouldBe(Guid.Empty);
            result.ValidationSummary.ShouldNotBeNull();
            result.ValidationSummary.IsValid.ShouldBeFalse();
            result.ValidationSummary.HasIndexHtml.ShouldBeFalse();
            result.ValidationSummary.Errors.ShouldContain(e => e.Contains("index.html"));
        }

        private static Stream CreateZipStream()
        {
            var stream = new MemoryStream();
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                var entry = zip.CreateEntry("index.html");
                using (var s = entry.Open())
                using (var writer = new StreamWriter(s))
                {
                    writer.Write("<html></html>");
                }
            }
            stream.Position = 0;
            return stream;
        }

        private static Stream CreateZipWithoutIndex()
        {
            var stream = new MemoryStream();
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                var entry = zip.CreateEntry("readme.txt");
                using (var s = entry.Open())
                using (var writer = new StreamWriter(s))
                {
                    writer.Write("No index here");
                }
            }
            stream.Position = 0;
            return stream;
        }

        private async Task<Guid> SeedGameAsync()
        {
            var profileId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            return await UsingDbContextAsync(async context =>
            {
                await context.DeveloperProfiles.AddAsync(new DeveloperProfile
                {
                    Id = profileId,
                    TenantId = AbpSession.TenantId,
                    UserId = AbpSession.UserId ?? 1,
                    DisplayName = "Tester",
                    Status = DeveloperProfileStatus.Active
                });

                await context.Games.AddAsync(new Game(gameId, "Build Test Game", "build-test-game", "Test game", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.InReview
                });

                await context.SaveChangesAsync();
                return gameId;
            });
        }
    }
}
