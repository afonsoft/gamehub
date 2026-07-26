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
        private readonly IDeveloperGameAppService _developerGameAppService;
        private readonly IModerationAppService _moderationAppService;
        private readonly IRepository<GameBuild, Guid> _buildRepository;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<UserReport, Guid> _reportRepository;

        public ModerationAppService_Tests()
        {
            _developerGameAppService = LocalIocManager.Resolve<IDeveloperGameAppService>();
            _moderationAppService = LocalIocManager.Resolve<IModerationAppService>();
            _buildRepository = LocalIocManager.Resolve<IRepository<GameBuild, Guid>>();
            _gameRepository = LocalIocManager.Resolve<IRepository<Game, Guid>>();
            _reportRepository = LocalIocManager.Resolve<IRepository<UserReport, Guid>>();
        }

        [Fact]
        public async Task Dado_BuildValidado_Quando_AprovarPeloDeveloper_Entao_StatusDeveSerApproved()
        {
            var buildId = SeedGameWithBuild("Developer Approve Game");

            await _developerGameAppService.ApproveBuildAsync(new DeveloperApproveBuildInput { GameBuildId = buildId });

            var updated = await _buildRepository.GetAsync(buildId);
            updated.Status.ShouldBe(GameBuildStatus.Approved);
        }

        [Fact]
        public async Task Dado_BuildValidado_Quando_RejeitarPeloDeveloper_Entao_StatusDeveSerRejected()
        {
            var buildId = SeedGameWithBuild("Developer Reject Game");

            await _developerGameAppService.RejectBuildAsync(new DeveloperRejectBuildInput
            {
                GameBuildId = buildId,
                Reason = "Does not meet guidelines."
            });

            var updated = await _buildRepository.GetAsync(buildId);
            updated.Status.ShouldBe(GameBuildStatus.Rejected);
        }

        [Fact]
        public async Task Dado_JogoSubmetido_Quando_CompletarReviewComAprovacao_Entao_BuildEPublicadoEJogoPublicado()
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
            build.Status.ShouldBe(GameBuildStatus.Published);
        }

        [Fact]
        public async Task Dado_JogoSubmetido_Quando_CompletarReviewComRejeicao_Entao_JogoFicaRejected()
        {
            var gameId = await SeedGameAndSubmitForReviewAsync("Reject Review Game");

            var reviews = await _moderationAppService.GetPendingReviewsAsync();
            var pending = reviews.Items.FirstOrDefault(r => r.GameId == gameId);
            pending.ShouldNotBeNull();

            await _moderationAppService.CompleteReviewAsync(new CompleteReviewInput
            {
                ReviewId = pending.ReviewId,
                Decision = ReviewDecision.Rejected,
                Notes = "Inappropriate content."
            });

            var game = await _gameRepository.GetAsync(gameId);
            game.Status.ShouldBe(GameStatus.Rejected);
        }

        [Fact]
        public async Task Dado_ReportAberto_Quando_AtualizarParaResolved_Entao_DefineDataDeResolucao()
        {
            var reportId = Guid.NewGuid();
            await UsingDbContextAsync(async context =>
            {
                await context.UserReports.AddAsync(new UserReport
                {
                    Id = reportId,
                    TenantId = AbpSession.TenantId,
                    GameId = Guid.NewGuid(),
                    UserId = AbpSession.UserId,
                    Reason = "abuse",
                    Status = UserReportStatus.Open
                });
                await context.SaveChangesAsync();
            });

            await _moderationAppService.UpdateReportStatusAsync(
                reportId,
                UserReportStatus.Resolved);

            var report = await _reportRepository.GetAsync(reportId);
            report.Status.ShouldBe(UserReportStatus.Resolved);
            report.ResolvedAt.ShouldNotBeNull();
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

                context.Games.Add(new Game(gameId, title, title.ToLowerInvariant().Replace(" ", "-"), "Test game", profileId)
                {
                    TenantId = AbpSession.TenantId
                });

                context.GameBuilds.Add(new GameBuild(buildId, gameId, "1.0.0", 1, "/uploads/test.zip", stream.Length, "hash")
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameBuildStatus.Validated
                });

                context.SaveChanges();
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

            await _developerGameAppService.ApproveBuildAsync(new DeveloperApproveBuildInput { GameBuildId = buildId });
            await _developerGameAppService.SubmitForReviewAsync(new SubmitGameForReviewInput { GameId = draft.Id, Notes = "" });
            return draft.Id;
        }
    }
}
