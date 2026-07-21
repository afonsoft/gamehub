using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub;
using GameHub.Admin;
using GameHub.Admin.Dto;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Developers;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class ModerationAppService_Tests : ProjectNameTestBase
    {
        private readonly IAdminGameAppService _adminGameAppService;
        private readonly IRepository<GameBuild, Guid> _buildRepository;

        public ModerationAppService_Tests()
        {
            _adminGameAppService = LocalIocManager.Resolve<IAdminGameAppService>();
            _buildRepository = LocalIocManager.Resolve<IRepository<GameBuild, Guid>>();
        }

        [Fact]
        public async Task Dado_BuildValido_Quando_Aprovar_Entao_StatusDeveSerApproved()
        {
            var buildId = SeedGameWithBuild("Moderation Game");

            await _adminGameAppService.ApproveBuildAsync(new ApproveBuildInput { GameBuildId = buildId });

            var updated = await _buildRepository.GetAsync(buildId);
            updated.Status.ShouldBe(GameBuildStatus.Approved);
        }

        [Fact]
        public async Task Dado_BuildReprovado_Quando_Reprovar_Entao_StatusDeveSerRejected()
        {
            var buildId = SeedGameWithBuild("Reject Game");

            await _adminGameAppService.RejectBuildAsync(new RejectBuildInput
            {
                GameBuildId = buildId,
                Reason = "Does not meet guidelines."
            });

            var updated = await _buildRepository.GetAsync(buildId);
            updated.Status.ShouldBe(GameBuildStatus.Rejected);
        }

        private Guid SeedGameWithBuild(string title)
        {
            var profileId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var buildId = Guid.NewGuid();

            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry("index.html");
                using (var writer = new StreamWriter(entry.Open()))
                {
                    writer.Write("<html></html>");
                }
            }

            stream.Position = 0;

            UsingDbContext(context =>
            {
                context.DeveloperProfiles.Add(new DeveloperProfile
                {
                    Id = profileId,
                    TenantId = AbpSession.TenantId,
                    UserId = AbpSession.UserId ?? 1,
                    DisplayName = "Tester",
                    Status = DeveloperProfileStatus.Active
                });

                context.Games.Add(new Game(gameId, title, title.ToLowerInvariant().Replace(" ", "-"), "Test game", profileId));

                context.GameBuilds.Add(new GameBuild(buildId, gameId, "1.0.0", 1, "/uploads/test.zip", stream.Length, "hash")
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameBuildStatus.Validated
                });
            });

            return buildId;
        }
    }
}
