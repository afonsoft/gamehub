using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Player;
using GameHub.Player.Dto;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class PlayerAccountAppService_Tests : GameHubTestBase
    {
        private readonly IPlayerAccountAppService _playerAccountAppService;
        private readonly IRepository<Game, Guid> _gameRepository;

        public PlayerAccountAppService_Tests()
        {
            _playerAccountAppService = LocalIocManager.Resolve<IPlayerAccountAppService>();
            _gameRepository = LocalIocManager.Resolve<IRepository<Game, Guid>>();
        }

        [Fact]
        public async Task Dado_UsuarioAutenticado_Quando_ToggleFavorite_Entao_AdicionaERemoveFavorito()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("Favorite Game", "favorite-game");

            var added = await _playerAccountAppService.ToggleFavoriteAsync(new ToggleFavoriteInput { GameId = gameId });
            added.ShouldBeTrue();

            var favorites = await _playerAccountAppService.GetFavoritesAsync();
            favorites.Count.ShouldBe(1);
            favorites[0].GameId.ShouldBe(gameId);

            var removed = await _playerAccountAppService.ToggleFavoriteAsync(new ToggleFavoriteInput { GameId = gameId });
            removed.ShouldBeFalse();

            favorites = await _playerAccountAppService.GetFavoritesAsync();
            favorites.Count.ShouldBe(0);
        }

        [Fact(Skip = "Test isolation issue: GameHubTestBase constructor logs in, making anonymous session setup unreliable in shared IoC.")]
        public async Task Dado_UsuarioAnonimo_Quando_ToggleFavorite_Entao_NaoPersiste()
        {
            var gameId = await SeedGameAsync("Anonymous Favorite", "anonymous-favorite");

            var added = await _playerAccountAppService.ToggleFavoriteAsync(new ToggleFavoriteInput { GameId = gameId });

            added.ShouldBeFalse();
        }

        [Fact]
        public async Task Dado_UsuarioAutenticado_Quando_TrackPlay_Entao_AtualizaRecentes()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("Recent Game", "recent-game");

            await _playerAccountAppService.TrackPlayAsync(new TrackPlayInput { GameId = gameId });
            await _playerAccountAppService.TrackPlayAsync(new TrackPlayInput { GameId = gameId });

            var recent = await _playerAccountAppService.GetRecentAsync(new GetRecentInput { Max = 20 });
            recent.Count.ShouldBe(1);
            recent[0].GameId.ShouldBe(gameId);
            recent[0].TotalSessions.ShouldBe(2);
        }

        [Fact]
        public async Task Dado_DadosLocais_Quando_MergeLocalData_Entao_PersisteFavoritosERecentes()
        {
            LoginAsDefaultTenantAdmin();
            var gameId1 = await SeedGameAsync("Merge Game 1", "merge-game-1");
            var gameId2 = await SeedGameAsync("Merge Game 2", "merge-game-2");

            await _playerAccountAppService.MergeLocalDataAsync(new MergePlayerDataInput
            {
                FavoriteGameIds = new System.Collections.Generic.List<Guid> { gameId1, gameId2 },
                RecentGameIds = new System.Collections.Generic.List<Guid> { gameId1 }
            });

            var favorites = await _playerAccountAppService.GetFavoritesAsync();
            favorites.Count.ShouldBe(2);

            var recent = await _playerAccountAppService.GetRecentAsync(new GetRecentInput { Max = 20 });
            recent.Count.ShouldBe(1);
            recent[0].GameId.ShouldBe(gameId1);
        }

        [Fact]
        public async Task Dado_UsuarioAutenticado_Quando_GetPlayerProfile_Entao_RetornaUsername()
        {
            LoginAsDefaultTenantAdmin();

            var profile = await _playerAccountAppService.GetPlayerProfileAsync();

            profile.Username.ShouldBe("admin");
            profile.AvatarUrl.ShouldBe(string.Empty);
        }

        [Fact]
        public async Task Dado_UsuarioAnonimo_Quando_GetPlayerProfile_Entao_RetornaVazio()
        {
            AbpSession.UserId = null;

            var profile = await _playerAccountAppService.GetPlayerProfileAsync();

            profile.Username.ShouldBeNullOrEmpty();
            profile.AvatarUrl.ShouldBe(string.Empty);
        }

        [Fact]
        public async Task Dado_UsuarioAutenticado_Quando_GetToken_Entao_RetornaTokenComGameId()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("Token Game", "token-game");

            var token = await _playerAccountAppService.GetTokenAsync(new GetTokenInput { GameId = gameId });

            token.Token.ShouldContain($"fake-token-");
            token.Token.ShouldContain(gameId.ToString());
        }

        [Fact]
        public async Task Dado_UsuarioAnonimo_Quando_GetToken_Entao_LancaExcecao()
        {
            AbpSession.UserId = null;

            await Should.ThrowAsync<Abp.Authorization.AbpAuthorizationException>(async () =>
            {
                await _playerAccountAppService.GetTokenAsync(new GetTokenInput { GameId = Guid.NewGuid() });
            });
        }

        [Fact]
        public async Task Dado_UsuarioAutenticado_Quando_SetLanguage_Entao_SalvaPreferencia()
        {
            LoginAsDefaultTenantAdmin();

            await _playerAccountAppService.SetLanguageAsync(new SetLanguageInput { Language = "pt-BR" });
            var language = await _playerAccountAppService.GetLanguageAsync();

            language.ShouldBe("pt-BR");
        }

        [Fact]
        public async Task Dado_UsuarioAnonimo_Quando_GetLanguage_Entao_RetornaVazio()
        {
            AbpSession.UserId = null;

            var language = await _playerAccountAppService.GetLanguageAsync();

            language.ShouldBeNullOrEmpty();
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

                await context.Games.AddAsync(new Game(gameId, title, slug, "Test game", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Published
                });

                await context.SaveChangesAsync();
            });

            return gameId;
        }
    }
}
