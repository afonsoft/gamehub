using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub.Admin;
using GameHub.Admin.Dto;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Developer;
using GameHub.Developer.Dto;
using GameHub.Developers;
using GameHub.Moderation;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class ModerationAppService_Tests : GameHubTestBase
    {
        private readonly IAdminGameAppService _adminGameAppService;
        private readonly IDeveloperGameAppService _developerGameAppService;
        private readonly IModerationAppService _moderationAppService;
        private readonly IRepository<GameBuild, Guid> _buildRepository;
        private readonly IRepository<Game, Guid> _gameRepository;

        public ModerationAppService_Tests()
        {
            _adminGameAppService = LocalIocManager.Resolve<IAdminGameAppService>();
            _developerGameAppService = LocalIocManager.Resolve<IDeveloperGameAppService>();
            _moderationAppService = LocalIocManager.Resolve<IModerationAppService>();
            _buildRepository = LocalIocManager.Resolve<IRepository<GameBuild, Guid>>();
            _gameRepository = LocalIocManager.Resolve<IRepository<Game, Guid>>();
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

        [Fact]
        public async Task Dado_JogoSubmetido_Quando_CompletarReviewComAprovacao_Entao_BuildEAprovadoEJogoPublicado()
        {
            var gameId = await SeedGameAndSubmitForReviewAsync("Approve Review Game");

            var reviews = await _moderationAppService.GetPendingReviewsAsync();
            var pending = reviews.Items.FirstOrDefault(r => r.GameId == gameId);
            pending.ShouldNotBeNull();

            await _moderationAppService.CompleteReviewAsync(new CompleteReviewInput
            {
                ReviewId = pending.ReviewId,
                Decision = ReviewDecision.Approved,
                Notes = "Looks good."
            });

            var game = await _gameRepository.GetAsync(gameId);
            game.Status.ShouldBe(GameStatus.Published);

            var build = await _buildRepository.GetAsync(pending.GameBuildId);
            build.Status.ShouldBe(GameBuildStatus.Approved);
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

        private async Task<Guid> SeedGameAndSubmitForReviewAsync(string title)
        {
            var draft = await _developerGameAppService.CreateDraftAsync(new CreateGameDraftInput
            {
                Title = title,
                ShortDescription = "For moderation",
                AgeRating = "E",
                Orientation = "Both"
            });

            var buildId = Guid.NewGuid();
            await UsingDbContextAsync(async context =>
            {
                await context.GameBuilds.AddAsync(new GameBuild(buildId, draft.Id, "1.0.0", 1, "/uploads/test.zip", 100, "hash")
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameBuildStatus.Validated
                });
                await context.SaveChangesAsync();
            });

            await _developerGameAppService.SubmitForReviewAsync(new SubmitGameForReviewInput { GameId = draft.Id, Notes = "" });
            return draft.Id;
        }
    }
}
