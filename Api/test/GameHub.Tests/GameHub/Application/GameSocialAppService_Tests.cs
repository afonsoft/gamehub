using System;
using System.Threading.Tasks;
using Eaf.Middleware.Authorization.Users;
using GameHub.Social;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class GameSocialAppService_Tests : GameHubTestBase
    {
        private readonly IGameSocialAppService _socialAppService;

        public GameSocialAppService_Tests()
        {
            _socialAppService = LocalIocManager.Resolve<IGameSocialAppService>();
        }

        [Fact]
        public async Task Dado_ConviteParaParticipante_Quando_Enviar_Entao_CriaNotificacao()
        {
            LoginAsDefaultTenantAdmin();
            var (gameId, matchId, targetUserId) = await SeedMatchAsync();

            var result = await _socialAppService.InvitePlayerAsync(new InvitePlayerInput
            {
                GameId = gameId,
                MatchId = matchId,
                InviteeUserId = targetUserId
            });

            result.InviteId.ShouldNotBe(Guid.Empty);
            var notifications = await UsingDbContextAsync(context =>
                context.GameNotifications.ToListAsync());
            notifications.ShouldContain(item => item.NotificationType == "match_invite");
        }

        [Fact]
        public async Task Dado_ConviteExpirado_Quando_Aceitar_Entao_Rejeita()
        {
            LoginAsDefaultTenantAdmin();
            var (gameId, matchId, targetUserId) = await SeedMatchAsync();
            var invite = await _socialAppService.InvitePlayerAsync(new InvitePlayerInput
            {
                GameId = gameId,
                MatchId = matchId,
                InviteeUserId = targetUserId
            });

            await UsingDbContextAsync(async context =>
            {
                var entity = await context.GameInvites.SingleAsync(item => item.Id == invite.InviteId);
                entity.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
                await context.SaveChangesAsync();
            });

            await Should.ThrowAsync<InvalidOperationException>(() =>
                _socialAppService.AcceptInviteAsync(invite.InviteId));
        }

        private async Task<(Guid GameId, Guid MatchId, long TargetUserId)> SeedMatchAsync()
        {
            var gameId = Guid.NewGuid();
            var matchId = Guid.NewGuid();
            long targetUserId = 0;

            await UsingDbContextAsync(async context =>
            {
                await context.Games.AddAsync(new global::GameHub.Catalog.Game(
                    gameId,
                    "Social Game",
                    "social-game",
                    "Social test game",
                    Guid.NewGuid())
                {
                    TenantId = AbpSession.TenantId
                });

                await context.MatchStates.AddAsync(new global::GameHub.Multiplayer.MatchState(
                    matchId,
                    gameId,
                    "SOCIAL01",
                    "casual",
                    4)
                {
                    TenantId = AbpSession.TenantId,
                    ExpiresAt = DateTime.UtcNow.AddHours(1)
                });

                await context.MatchParticipants.AddAsync(new global::GameHub.Multiplayer.MatchParticipant
                {
                    Id = Guid.NewGuid(),
                    TenantId = AbpSession.TenantId,
                    MatchId = matchId,
                    UserId = AbpSession.UserId,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow
                });

                var targetUser = new User
                {
                    TenantId = AbpSession.TenantId,
                    UserName = $"social-{Guid.NewGuid():N}",
                    Name = "Social",
                    Surname = "Target",
                    EmailAddress = $"social-{Guid.NewGuid():N}@example.test",
                    IsActive = true,
                    IsEmailConfirmed = true,
                    Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw=="
                };
                targetUser.SetNormalizedNames();
                await context.Users.AddAsync(targetUser);
                await context.SaveChangesAsync();
                targetUserId = targetUser.Id;

                await context.MatchParticipants.AddAsync(new global::GameHub.Multiplayer.MatchParticipant
                {
                    Id = Guid.NewGuid(),
                    TenantId = AbpSession.TenantId,
                    MatchId = matchId,
                    UserId = targetUserId,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            });

            return (gameId, matchId, targetUserId);
        }
    }
}
