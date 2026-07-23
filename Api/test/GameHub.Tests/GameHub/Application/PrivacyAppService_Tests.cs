using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Gameplay;
using GameHub.Moderation;
using GameHub.Privacy;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class PrivacyAppService_Tests : GameHubTestBase
    {
        private readonly IPrivacyAppService _privacyAppService;

        public PrivacyAppService_Tests()
        {
            _privacyAppService = Resolve<IPrivacyAppService>();
        }

        [Fact]
        public async Task Dado_UsuarioComDados_Quando_Exportar_Entao_DeveRetornarDadosPessoais()
        {
            var userId = AbpSession.UserId.Value;
            var gameId = Guid.NewGuid();
            var profileId = Guid.NewGuid();
            var date = DateTime.UtcNow.Date;

            await UsingDbContextAsync(async context =>
            {
                await context.DeveloperProfiles.AddAsync(new DeveloperProfile
                {
                    Id = profileId,
                    TenantId = AbpSession.TenantId,
                    UserId = userId,
                    DisplayName = "Dev User",
                    LegalName = "Legal Name",
                    SupportEmail = "dev@example.com",
                    Status = DeveloperProfileStatus.Active
                });

                await context.Games.AddAsync(new Game(gameId, "Privacy Test Game", "privacy-test-game", "Test", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Published
                });

                await context.PlaySessions.AddAsync(new PlaySession
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    UserId = userId,
                    StartedAt = date.AddHours(1),
                    DeviceType = "Desktop",
                    Browser = "Chrome",
                    TenantId = AbpSession.TenantId,
                });

                await context.LeaderboardEntries.AddAsync(new LeaderboardEntry
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    UserId = userId,
                    Score = 100,
                    CreatedAt = date,
                    UpdatedAt = date,
                    TenantId = AbpSession.TenantId,
                });

                await context.UserReports.AddAsync(new UserReport
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    UserId = userId,
                    Reason = "Cheating",
                    Description = "Suspicious behavior",
                    Status = UserReportStatus.Open,
                    TenantId = AbpSession.TenantId,
                });
            });

            var export = await _privacyAppService.ExportUserDataAsync(userId);

            export.ShouldNotBeNull();
            export.UserId.ShouldBe(userId);
            export.PlaySessions.Count.ShouldBe(1);
            export.LeaderboardEntries.Count.ShouldBe(1);
            export.UserReports.Count.ShouldBe(1);
            export.DeveloperProfile.ShouldNotBeNull();
            export.DeveloperProfile.DisplayName.ShouldBe("Dev User");
        }

        [Fact]
        public async Task Dado_UsuarioComDados_Quando_Deletar_Entao_DeveAnonimizar()
        {
            var userId = AbpSession.UserId.Value;
            var gameId = Guid.NewGuid();
            var profileId = Guid.NewGuid();
            var date = DateTime.UtcNow.Date;

            await UsingDbContextAsync(async context =>
            {
                await context.DeveloperProfiles.AddAsync(new DeveloperProfile
                {
                    Id = profileId,
                    TenantId = AbpSession.TenantId,
                    UserId = userId,
                    DisplayName = "Dev User",
                    SupportEmail = "dev@example.com",
                    Status = DeveloperProfileStatus.Active
                });

                await context.Games.AddAsync(new Game(gameId, "Delete Test Game", "delete-test-game", "Test", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Published
                });

                await context.PlaySessions.AddAsync(new PlaySession
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    UserId = userId,
                    StartedAt = date.AddHours(1),
                    DeviceType = "Desktop",
                    Browser = "Chrome",
                    TenantId = AbpSession.TenantId,
                });

                await context.UserReports.AddAsync(new UserReport
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    UserId = userId,
                    Reason = "Cheating",
                    Status = UserReportStatus.Open,
                    TenantId = AbpSession.TenantId,
                });
            });

            await _privacyAppService.DeleteUserDataAsync(userId);

            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FindAsync(userId);
                user.ShouldNotBeNull();
                user.Name.ShouldBe("Deleted User");
                user.IsDeleted.ShouldBeTrue();
                user.IsActive.ShouldBeFalse();

                var profile = await context.DeveloperProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                profile.ShouldNotBeNull();
                profile.DisplayName.ShouldBe("Deleted User");
                profile.Status.ShouldBe(DeveloperProfileStatus.Suspended);

                var sessions = await context.PlaySessions.Where(s => s.UserId == userId).ToListAsync();
                sessions.Count.ShouldBe(0);

                var reports = await context.UserReports.Where(r => r.UserId == userId).ToListAsync();
                reports.Count.ShouldBe(0);
            });
        }
    }
}
