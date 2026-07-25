using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Castle.MicroKernel.Registration;
using GameHub.Builds;
using GameHub.Builds.Dto;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Storage;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class GameBuildAppService_Tests : GameHubTestBase
    {
        private readonly IGameBuildAppService _gameBuildAppService;
        private readonly IBuildValidationAppService _buildValidationAppService;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<GameBuild, Guid> _buildRepository;
        private readonly IRepository<BuildValidationReport, Guid> _reportRepository;

        public GameBuildAppService_Tests()
        {
            _gameBuildAppService = LocalIocManager.Resolve<IGameBuildAppService>();
            _buildValidationAppService = LocalIocManager.Resolve<IBuildValidationAppService>();
            _gameRepository = LocalIocManager.Resolve<IRepository<Game, Guid>>();
            _buildRepository = LocalIocManager.Resolve<IRepository<GameBuild, Guid>>();
            _reportRepository = LocalIocManager.Resolve<IRepository<BuildValidationReport, Guid>>();
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

        [Fact]
        public async Task Dado_PackageValido_Quando_UploadBuild_Entao_PersisteRelatorioDeValidacao()
        {
            var gameId = await SeedGameAsync();
            using var stream = CreateZipStream();

            var result = await _gameBuildAppService.UploadBuildAsync(gameId, stream, "build.zip", "application/zip");

            var report = await _buildValidationAppService.GetReportAsync(result.BuildId);
            report.ShouldNotBeNull();
            report.IsValid.ShouldBeTrue();
            report.GameBuildId.ShouldBe(result.BuildId);
            report.Errors.ShouldBeEmpty();
        }

        [Fact]
        public async Task Dado_PackageComUrlExterna_Quando_UploadBuild_Entao_GeraWarning()
        {
            var gameId = await SeedGameAsync();
            using var stream = CreateZipWithExternalUrl();

            var result = await _gameBuildAppService.UploadBuildAsync(gameId, stream, "build.zip", "application/zip");

            result.Status.ShouldBe(GameBuildStatus.Validated.ToString());
            result.ValidationSummary.Warnings.ShouldContain(w => w.Contains("External requests found"));

            var report = await _buildValidationAppService.GetReportAsync(result.BuildId);
            report.Warnings.ShouldContain(w => w.Contains("External requests found"));
        }

        [Fact]
        public async Task Dado_PackageValido_Quando_UploadBuild_Entao_RelatorioApareceNaLista()
        {
            var gameId = await SeedGameAsync();
            using var stream = CreateZipStream();

            var result = await _gameBuildAppService.UploadBuildAsync(gameId, stream, "build.zip", "application/zip");

            var reports = await _buildValidationAppService.GetReportsAsync(10);
            reports.ShouldContain(r => r.GameBuildId == result.BuildId && r.GameTitle == "Build Test Game");
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

        private static Stream CreateZipWithExternalUrl()
        {
            var stream = new MemoryStream();
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                var entry = zip.CreateEntry("index.html");
                using (var s = entry.Open())
                using (var writer = new StreamWriter(s))
                {
                    writer.Write("<html><script src=\"https://example.com/script.js\"></script></html>");
                }
            }
            stream.Position = 0;
            return stream;
        }

        private static byte[] ReadStreamToBytes(Stream stream)
        {
            var bytes = new byte[stream.Length];
            stream.ReadExactly(bytes, 0, bytes.Length);
            return bytes;
        }

        [Fact]
        public async Task Dado_ApiKeyValida_Quando_UploadFromCli_Entao_PersisteBuildParaJogo()
        {
            var (gameId, slug, apiKey) = await SeedGameWithApiKeyAsync("cli-test-game");
            using var stream = CreateZipStream();
            var package = ReadStreamToBytes(stream);

            var result = await _gameBuildAppService.UploadFromCliAsync(new UploadFromCliInput
            {
                ApiKey = apiKey,
                GameSlug = slug,
                Version = "1.2.3",
                Package = package
            });

            result.ShouldNotBeNull();
            result.Status.ShouldBe(GameBuildStatus.Validated.ToString());
            result.Version.ShouldBe("1.2.3");
            result.BuildId.ShouldNotBe(Guid.Empty);

            var build = await _buildRepository.GetAsync(result.BuildId);
            build.GameId.ShouldBe(gameId);
        }

        [Fact]
        public async Task Dado_ApiKeyInvalida_Quando_UploadFromCli_Entao_RetornaErro()
        {
            var (_, slug, _) = await SeedGameWithApiKeyAsync("cli-test-game-2");
            using var stream = CreateZipStream();
            var package = ReadStreamToBytes(stream);

            var result = await _gameBuildAppService.UploadFromCliAsync(new UploadFromCliInput
            {
                ApiKey = "invalid-key",
                GameSlug = slug,
                Package = package
            });

            result.ShouldNotBeNull();
            result.Status.ShouldBe(GameBuildStatus.ValidationFailed.ToString());
            result.ValidationSummary.Errors.ShouldContain(e => e.Contains("Invalid API key"));
        }

        private async Task<(Guid gameId, string slug, string apiKey)> SeedGameWithApiKeyAsync(string slug)
        {
            var profileId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var apiKey = $"gh_cli_{Guid.NewGuid():N}";

            return await UsingDbContextAsync(async context =>
            {
                await context.DeveloperProfiles.AddAsync(new DeveloperProfile
                {
                    Id = profileId,
                    TenantId = AbpSession.TenantId,
                    UserId = AbpSession.UserId ?? 1,
                    DisplayName = "CLI Tester",
                    Status = DeveloperProfileStatus.Active,
                    ApiKey = apiKey
                });

                await context.Games.AddAsync(new Game(gameId, "CLI Test Game", slug, "Test game for CLI", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.InReview
                });

                await context.SaveChangesAsync();
                return (gameId, slug, apiKey);
            });
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
