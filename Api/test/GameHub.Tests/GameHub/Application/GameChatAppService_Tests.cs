using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub.Catalog;
using GameHub.Multiplayer;
using GameHub.Chat;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class GameChatAppService_Tests : GameHubTestBase
    {
        private readonly IGameChatAppService _chatAppService;

        public GameChatAppService_Tests()
        {
            _chatAppService = LocalIocManager.Resolve<IGameChatAppService>();
        }

        [Fact]
        public async Task Dado_UsuarioForaDaPartida_Quando_EnviarMensagem_Entao_Rejeita()
        {
            LoginAsDefaultTenantAdmin();
            var (gameId, matchId) = await SeedMatchAsync("Chat Authorization Game");

            await Should.ThrowAsync<InvalidOperationException>(() =>
                _chatAppService.SendAsync(new SendGameChatMessageInput
                {
                    GameId = gameId,
                    ConversationId = $"match:{matchId}",
                    Text = "hello",
                    ClientMessageId = Guid.NewGuid().ToString("N")
                }));
        }

        [Fact]
        public async Task Dado_MensagemRepetida_Quando_EnviarNovamente_Entao_RetornaDuplicada()
        {
            LoginAsDefaultTenantAdmin();
            var (gameId, matchId) = await SeedMatchAsync("Chat Deduplication Game", includeCurrentUser: true);
            var clientMessageId = Guid.NewGuid().ToString("N");

            var first = await _chatAppService.SendAsync(new SendGameChatMessageInput
            {
                GameId = gameId,
                ConversationId = $"match:{matchId}",
                Text = "hello",
                ClientMessageId = clientMessageId
            });
            var second = await _chatAppService.SendAsync(new SendGameChatMessageInput
            {
                GameId = gameId,
                ConversationId = $"match:{matchId}",
                Text = "hello",
                ClientMessageId = clientMessageId
            });

            first.Duplicate.ShouldBeFalse();
            second.Duplicate.ShouldBeTrue();
        }

        private async Task<(Guid GameId, Guid MatchId)> SeedMatchAsync(string title, bool includeCurrentUser = false)
        {
            var gameId = Guid.NewGuid();
            var matchId = Guid.NewGuid();

            await UsingDbContextAsync(async context =>
            {
                await context.Games.AddAsync(new Game(gameId, title, title.ToLowerInvariant().Replace(" ", "-"), "Chat game", Guid.NewGuid())
                {
                    TenantId = AbpSession.TenantId,
                    SupportsMultiplayer = true
                });
                await context.MatchStates.AddAsync(new MatchState(matchId, gameId, "ROOM01", "casual", 4)
                {
                    TenantId = AbpSession.TenantId,
                    ExpiresAt = DateTime.UtcNow.AddHours(1)
                });
                if (includeCurrentUser)
                {
                    await context.MatchParticipants.AddAsync(new MatchParticipant
                    {
                        Id = Guid.NewGuid(),
                        TenantId = AbpSession.TenantId,
                        MatchId = matchId,
                        UserId = AbpSession.UserId,
                        IsActive = true,
                        JoinedAt = DateTime.UtcNow
                    });
                }
                await context.SaveChangesAsync();
            });

            return (gameId, matchId);
        }
    }
}
