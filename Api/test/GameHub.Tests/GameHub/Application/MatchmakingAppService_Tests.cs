using System;
using System.Threading.Tasks;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Multiplayer;
using GameHub.Multiplayer.Dto;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class MatchmakingAppService_Tests : GameHubTestBase
    {
        private readonly IMultiplayerAppService _multiplayerAppService;

        public MatchmakingAppService_Tests()
        {
            _multiplayerAppService = LocalIocManager.Resolve<IMultiplayerAppService>();
        }

        [Fact]
        public async Task Dado_JogoMultiplayer_Quando_CriarMatch_Entao_RetornaSalaComCodigo()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("Multiplayer Game", "multiplayer-game");

            var match = await _multiplayerAppService.CreateMatchAsync(new CreateMatchInput
            {
                GameId = gameId,
                Mode = "1v1",
                MaxPlayers = 2
            });

            match.ShouldNotBeNull();
            match.GameId.ShouldBe(gameId);
            match.RoomCode.ShouldNotBeNullOrWhiteSpace();
            match.Status.ShouldBe(MatchStatus.Waiting.ToString());
            match.MaxPlayers.ShouldBe(2);
        }

        [Fact]
        public async Task Dado_SalaComVaga_Quando_JoinMatch_Entao_JogadorEntra()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("Multiplayer Game", "multiplayer-game-join");
            var created = await _multiplayerAppService.CreateMatchAsync(new CreateMatchInput
            {
                GameId = gameId,
                MaxPlayers = 2
            });

            var joined = await _multiplayerAppService.JoinMatchAsync(new JoinMatchInput
            {
                MatchId = created.Id,
                ConnectionId = "conn-1"
            });

            joined.ShouldNotBeNull();
            joined.Id.ShouldBe(created.Id);
            joined.Participants.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Dado_SalaCheia_Quando_JoinMatch_Entao_LancaExcecao()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("Multiplayer Game", "multiplayer-game-full");
            var created = await _multiplayerAppService.CreateMatchAsync(new CreateMatchInput
            {
                GameId = gameId,
                MaxPlayers = 2
            });

            await _multiplayerAppService.JoinMatchAsync(new JoinMatchInput
            {
                MatchId = created.Id,
                ConnectionId = "conn-1"
            });

            await _multiplayerAppService.JoinMatchAsync(new JoinMatchInput
            {
                MatchId = created.Id,
                ConnectionId = "conn-2"
            });

            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await _multiplayerAppService.JoinMatchAsync(new JoinMatchInput
                {
                    MatchId = created.Id,
                    ConnectionId = "conn-3"
                });
            });
        }

        [Fact]
        public async Task Dado_JogoSemMultiplayer_Quando_CriarMatch_Entao_LancaExcecao()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("Single Game", "single-game");
            await UsingDbContextAsync(async context =>
            {
                var game = await context.Games.FindAsync(gameId);
                game.SupportsMultiplayer = false;
                game.MaxPlayersPerMatch = 0;
            });

            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await _multiplayerAppService.CreateMatchAsync(new CreateMatchInput { GameId = gameId });
            });
        }

        [Fact]
        public async Task Dado_SalaAtiva_Quando_JoinPorRoomCode_Entao_JogadorEntra()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("Multiplayer Game", "multiplayer-game-room");
            var created = await _multiplayerAppService.CreateMatchAsync(new CreateMatchInput
            {
                GameId = gameId,
                MaxPlayers = 2
            });

            var joined = await _multiplayerAppService.JoinMatchByRoomCodeAsync(new JoinMatchByRoomCodeInput
            {
                RoomCode = created.RoomCode,
                ConnectionId = "conn-room"
            });

            joined.ShouldNotBeNull();
            joined.RoomCode.ShouldBe(created.RoomCode);
            joined.Participants.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Dado_SalaAtiva_Quando_LeaveMatch_Entao_ParticipanteDesativado()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("Multiplayer Game", "multiplayer-game-leave");
            var created = await _multiplayerAppService.CreateMatchAsync(new CreateMatchInput
            {
                GameId = gameId,
                MaxPlayers = 2
            });

            await _multiplayerAppService.JoinMatchAsync(new JoinMatchInput
            {
                MatchId = created.Id,
                ConnectionId = "conn-leave"
            });

            await _multiplayerAppService.LeaveMatchAsync(new LeaveMatchInput
            {
                MatchId = created.Id,
                ConnectionId = "conn-leave"
            });

            var match = await _multiplayerAppService.GetMatchAsync(created.Id);
            match.Participants.Count.ShouldBe(1);
            match.Participants.ShouldContain(p => p.ConnectionId == "conn-leave" && !p.IsActive);
        }

        [Fact]
        public async Task Dado_SalaEmEspera_Quando_EndMatch_Entao_StatusFinalizado()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("Multiplayer Game", "multiplayer-game-end");
            var created = await _multiplayerAppService.CreateMatchAsync(new CreateMatchInput
            {
                GameId = gameId,
                MaxPlayers = 2
            });

            await _multiplayerAppService.EndMatchAsync(created.Id);

            var match = await _multiplayerAppService.GetMatchAsync(created.Id);
            match.Status.ShouldBe(MatchStatus.Ended.ToString());
        }

        [Fact]
        public async Task Dado_SalaEmAndamento_Quando_UpdateMatchState_Entao_AtualizaPayload()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("Multiplayer Game", "multiplayer-game-state");
            var created = await _multiplayerAppService.CreateMatchAsync(new CreateMatchInput
            {
                GameId = gameId,
                MaxPlayers = 2
            });

            await _multiplayerAppService.UpdateMatchStateAsync(new UpdateMatchStateInput
            {
                MatchId = created.Id,
                PayloadJson = "{\"score\":10}"
            });

            var match = await _multiplayerAppService.GetMatchAsync(created.Id);
            match.PayloadJson.ShouldBe("{\"score\":10}");
        }

        [Fact]
        public async Task Dado_MatchmakingAberto_Quando_CreateOrJoinMatch_Entao_CriaOuRetornaSala()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("Multiplayer Game", "multiplayer-game-coj");
            var match = await _multiplayerAppService.CreateOrJoinMatchAsync(new CreateMatchInput
            {
                GameId = gameId,
                MaxPlayers = 2
            });

            match.ShouldNotBeNull();
            match.GameId.ShouldBe(gameId);

            var same = await _multiplayerAppService.CreateOrJoinMatchAsync(new CreateMatchInput
            {
                GameId = gameId,
                MaxPlayers = 2
            });

            same.Id.ShouldBe(match.Id);
        }

        private async Task<Guid> SeedGameAsync(string title, string slug)
        {
            var gameId = Guid.NewGuid();
            var profileId = Guid.NewGuid();
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

                var game = new Game(gameId, title, slug, "Test", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Draft,
                    SupportsMultiplayer = true,
                    MaxPlayersPerMatch = 4
                };
                await context.Games.AddAsync(game);
            });

            return gameId;
        }
    }
}
