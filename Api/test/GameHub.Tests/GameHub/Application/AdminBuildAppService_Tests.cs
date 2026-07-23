using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Castle.MicroKernel.Registration;
using GameHub.Admin;
using GameHub.Admin.Dto;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Storage;
using NSubstitute;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class AdminBuildAppService_Tests : GameHubTestBase
    {
        private readonly IAdminBuildAppService _adminBuildAppService;
        private readonly IRepository<GameBuild, Guid> _buildRepository;
        private readonly IRepository<Game, Guid> _gameRepository;

        public AdminBuildAppService_Tests()
        {
            LocalIocManager.IocContainer.Register(
                Component.For<IGameAssetStorage>().UsingFactoryMethod(() =>
                {
                    var substitute = Substitute.For<IGameAssetStorage>();
                    substitute.ListBuildFilesAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                        .Returns(Task.FromResult<IReadOnlyList<StoredFile>>(new List<StoredFile>
                        {
                            new StoredFile
                            {
                                Key = "builds/game/build/index.html",
                                Name = "index.html",
                                SizeBytes = 100,
                                Url = "http://minio/gamehub/builds/game/build/index.html",
                                ContentType = "text/html"
                            }
                        }));
                    return substitute;
                }).LifestyleSingleton());

            _adminBuildAppService = LocalIocManager.Resolve<IAdminBuildAppService>();
            _buildRepository = LocalIocManager.Resolve<IRepository<GameBuild, Guid>>();
            _gameRepository = LocalIocManager.Resolve<IRepository<Game, Guid>>();
        }

        [Fact]
        public async Task Dado_BuildExistente_Quando_ListarUploads_Entao_RetornaItemComGameEDeveloper()
        {
            var buildId = await SeedBuildAsync("List Build Game");

            var result = await _adminBuildAppService.GetAllBuildsAsync(new GetBuildsInput { SkipCount = 0, MaxResultCount = 10 });

            result.TotalCount.ShouldBeGreaterThan(0);
            result.Items.ShouldContain(b => b.Id == buildId);
        }

        [Fact]
        public async Task Dado_BuildExistente_Quando_ListarArquivos_Entao_RetornaIndexHtmlMarcado()
        {
            var buildId = await SeedBuildAsync("Files Build Game");
            var build = await _buildRepository.GetAsync(buildId);

            var result = await _adminBuildAppService.GetBuildFilesAsync(buildId);

            result.Items.Count.ShouldBe(1);
            result.Items[0].Name.ShouldBe("index.html");
            result.Items[0].IsIndexHtml.ShouldBeTrue();
            result.Items[0].ContentType.ShouldBe("text/html");
        }

        private async Task<Guid> SeedBuildAsync(string title)
        {
            var profileId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var buildId = Guid.NewGuid();

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
                    Status = GameStatus.Draft
                });

                await context.GameBuilds.AddAsync(new GameBuild(buildId, gameId, "1.0.0", 1, "/uploads/test.zip", 100, "hash")
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameBuildStatus.Validated,
                    IndexHtmlPath = "index.html"
                });

                await context.SaveChangesAsync();
            });

            return buildId;
        }
    }
}
