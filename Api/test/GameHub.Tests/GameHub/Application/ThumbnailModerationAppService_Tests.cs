using System;
using System.Threading.Tasks;
using GameHub.Admin;
using GameHub.Catalog;
using GameHub.Developers;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class ThumbnailModerationAppService_Tests : GameHubTestBase
    {
        private readonly IAdminGameAppService _adminGameAppService;

        public ThumbnailModerationAppService_Tests()
        {
            _adminGameAppService = Resolve<IAdminGameAppService>();
        }

        [Fact]
        public async Task Dado_ThumbnailPendente_Quando_Aprovar_Entao_StatusDeveSerApproved()
        {
            var gameId = await CriarJogoComAnimatedThumbnailAsync();

            await _adminGameAppService.ApproveThumbnailAsync(gameId);

            var game = await ObterJogoAsync(gameId);
            game.ThumbnailStatus.ShouldBe(GameThumbnailStatus.Approved);
        }

        [Fact]
        public async Task Dado_ThumbnailPendente_Quando_Rejeitar_Entao_StatusDeveSerRejected()
        {
            var gameId = await CriarJogoComAnimatedThumbnailAsync();

            await _adminGameAppService.RejectThumbnailAsync(gameId);

            var game = await ObterJogoAsync(gameId);
            game.ThumbnailStatus.ShouldBe(GameThumbnailStatus.Rejected);
        }

        private async Task<Guid> CriarJogoComAnimatedThumbnailAsync()
        {
            var gameId = Guid.NewGuid();
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

                await context.Games.AddAsync(new Game(gameId, "Thumb Test", "thumb-test", "Test", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Draft,
                    ThumbnailUrl = "https://cdn.test/thumb.png",
                    AnimatedThumbnailUrl = "https://cdn.test/thumb.gif",
                    ThumbnailStatus = GameThumbnailStatus.Pending
                });
            });

            return gameId;
        }

        private async Task<Game> ObterJogoAsync(Guid gameId)
        {
            return await UsingDbContextAsync(async context =>
                await context.Games.FirstOrDefaultAsync(g => g.Id == gameId));
        }
    }
}
