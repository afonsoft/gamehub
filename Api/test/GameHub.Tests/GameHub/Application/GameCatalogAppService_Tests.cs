using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Catalog.Dto;
using GameHub.Developer;
using GameHub.Developer.Dto;
using GameHub.Gameplay;
using GameHub.Monetization;
using GameHub.Playtesting;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class GameCatalogAppService_Tests : GameHubTestBase
    {
        private readonly IGameCatalogAppService _catalogAppService;
        private readonly IDeveloperGameAppService _developerGameAppService;
        private readonly IRevenueContractAppService _revenueContractAppService;
        private readonly IRepository<Game, Guid> _gameRepository;

        public GameCatalogAppService_Tests()
        {
            _catalogAppService = Resolve<IGameCatalogAppService>();
            _developerGameAppService = Resolve<IDeveloperGameAppService>();
            _revenueContractAppService = Resolve<IRevenueContractAppService>();
            _gameRepository = Resolve<IRepository<Game, Guid>>();
        }

        [Fact]
        public async Task Dado_JogoPublicado_Quando_BuscarPorSlug_Entao_RetornaDetalheECache()
        {
            var slug = await SeedPublishedGameAsync("Cached Game");

            var first = await _catalogAppService.GetBySlugAsync(slug);
            var second = await _catalogAppService.GetBySlugAsync(slug);

            first.ShouldNotBeNull();
            first.Title.ShouldBe("Cached Game");
            second.ShouldNotBeNull();
            second.Title.ShouldBe(first.Title);
        }

        [Fact]
        public async Task Dado_JogosPublicados_Quando_Pesquisar_Entao_RetornaResultadosComCache()
        {
            await SeedPublishedGameAsync("Searchable Adventure");

            var result = await _catalogAppService.SearchAsync(new SearchInput
            {
                Query = "adventure",
                SkipCount = 0,
                MaxResultCount = 10
            });

            result.ShouldNotBeNull();
            result.TotalCount.ShouldBeGreaterThan(0);
            result.Items.ShouldContain(g => g.Title == "Searchable Adventure");

            var cached = await _catalogAppService.SearchAsync(new SearchInput
            {
                Query = "adventure",
                SkipCount = 0,
                MaxResultCount = 10
            });

            cached.ShouldNotBeNull();
            cached.Items.Count.ShouldBe(result.Items.Count);
        }

        [Fact]
        public async Task Dado_HomeComJogos_Quando_Carregar_Entao_RetornaSecoes()
        {
            await SeedPublishedGameAsync("Trending Game One", totalPlays: 100);
            await SeedPublishedGameAsync("Trending Game Two", totalPlays: 50);

            var home = await _catalogAppService.GetHomeAsync();

            home.ShouldNotBeNull();
            home.NewGames.ShouldNotBeEmpty();
            home.MostPlayed.ShouldNotBeEmpty();
            home.Trending.ShouldNotBeEmpty();
            home.Categories.ShouldNotBeNull();
            home.MostPlayed.First().TotalPlays.ShouldBeGreaterThanOrEqualTo(home.MostPlayed.Last().TotalPlays);
        }

        [Fact]
        public async Task Dado_CategoriaExistente_Quando_ListarJogosPorCategoria_Entao_FiltraCorretamente()
        {
            var categoryId = await SeedCategoryAsync("Puzzle");
            var slug = await SeedPublishedGameAsync("Puzzle Master", categoryIds: new List<Guid> { categoryId });
            await SeedPublishedGameAsync("Action Master");

            var result = await _catalogAppService.GetGamesAsync(new GetGamesInput
            {
                CategorySlug = "puzzle",
                SkipCount = 0,
                MaxResultCount = 10
            });

            result.Items.Count.ShouldBe(1);
            result.Items.First().Slug.ShouldBe(slug);
        }

        [Fact]
        public async Task Dado_OrdenacaoPorTitulo_Quando_ListarJogos_Entao_RetornaOrdenado()
        {
            await SeedPublishedGameAsync("Zebra Game");
            await SeedPublishedGameAsync("Alpha Game");

            var result = await _catalogAppService.GetGamesAsync(new GetGamesInput
            {
                Sorting = "Title",
                SkipCount = 0,
                MaxResultCount = 10
            });

            result.Items.First().Title.ShouldBe("Alpha Game");
            result.Items.Last().Title.ShouldBe("Zebra Game");
        }

        [Fact]
        public async Task Dado_MetricasRecentes_Quando_CarregarHome_Entao_TrendingOrdenaPorPontuacao()
        {
            var highScoreSlug = await SeedPublishedGameAsync("High Score Game");
            var lowScoreSlug = await SeedPublishedGameAsync("Low Score Game");

            await UsingDbContextAsync(async context =>
            {
                var highGame = await context.Games.FirstAsync(g => g.Slug == highScoreSlug);
                var lowGame = await context.Games.FirstAsync(g => g.Slug == lowScoreSlug);

                await context.GameMetricSnapshots.AddRangeAsync(
                    new GameMetricSnapshot
                    {
                        Id = Guid.NewGuid(),
                        GameId = highGame.Id,
                        TenantId = AbpSession.TenantId,
                        Date = DateTime.Now.AddDays(-1).Date,
                        Plays = 200,
                        UniquePlayers = 100,
                        AvgDurationSeconds = 120,
                        LoadingFinishedCount = 200,
                        ErrorCount = 0,
                        CommercialBreakCount = 0,
                        RewardedBreakCount = 0
                    },
                    new GameMetricSnapshot
                    {
                        Id = Guid.NewGuid(),
                        GameId = lowGame.Id,
                        TenantId = AbpSession.TenantId,
                        Date = DateTime.Now.AddDays(-1).Date,
                        Plays = 10,
                        UniquePlayers = 5,
                        AvgDurationSeconds = 60,
                        LoadingFinishedCount = 10,
                        ErrorCount = 0,
                        CommercialBreakCount = 0,
                        RewardedBreakCount = 0
                    });
            });

            var home = await _catalogAppService.GetHomeAsync();

            home.Trending.First().Slug.ShouldBe(highScoreSlug);
            home.Trending.Last().Slug.ShouldBe(lowScoreSlug);
            home.PopularThisWeek.First().Slug.ShouldBe(highScoreSlug);
            home.PopularThisWeek.Last().Slug.ShouldBe(lowScoreSlug);
            home.TopFree.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task Dado_JogoPublicado_Quando_BuscarPorSlug_Entao_RetornaJogosRelacionados()
        {
            var categoryId = await SeedCategoryAsync("Action");
            var slug = await SeedPublishedGameAsync("Action One", categoryIds: new List<Guid> { categoryId });
            await SeedPublishedGameAsync("Action Two", categoryIds: new List<Guid> { categoryId });

            var detail = await _catalogAppService.GetBySlugAsync(slug);

            detail.ShouldNotBeNull();
            detail.RelatedGames.ShouldNotBeEmpty();
            detail.RelatedGames.Count.ShouldBeLessThanOrEqualTo(6);
        }

        [Fact]
        public async Task Dado_VotoEmJogo_Quando_Recalcular_Entao_AverageRatingAtualizado()
        {
            var slug = await SeedPublishedGameAsync("Rated Game");
            var gameId = await UsingDbContextAsync(async context =>
            {
                var game = await context.Games.FirstAsync(g => g.Slug == slug);
                return game.Id;
            });

            await _catalogAppService.VoteAsync(new GameVoteInput
            {
                GameId = gameId,
                VoteType = GameVoteType.Like,
                DeviceId = "test-device"
            });

            var rating = await UsingDbContextAsync(async context =>
            {
                var game = await context.Games.FirstAsync(g => g.Id == gameId);
                return game.AverageRating;
            });

            rating.ShouldNotBeNull();
            rating.Value.ShouldBe(5.0, 0.01);
        }

        [Fact]
        public async Task Dado_ContratoWebExclusive_Quando_CarregarHome_Entao_JogoApareceEmWebExclusives()
        {
            var slug = await SeedPublishedGameAsync("Exclusive Web Game");
            var gameId = await UsingDbContextAsync(async context =>
            {
                var game = await context.Games.FirstAsync(g => g.Slug == slug);
                return game.Id;
            });

            await _revenueContractAppService.SetContractAsync(gameId, RevenueContractType.WebExclusive);

            var home = await _catalogAppService.GetHomeAsync();

            home.WebExclusives.ShouldNotBeEmpty();
            home.WebExclusives.ShouldContain(g => g.Slug == slug);
        }

        [Fact]
        public async Task Dado_FiltroExclusividade_Quando_ListarJogos_Entao_RetornaApenasExclusivos()
        {
            var exclusiveSlug = await SeedPublishedGameAsync("Exclusive Filter Game");
            var normalSlug = await SeedPublishedGameAsync("Normal Filter Game");

            var exclusiveGameId = await UsingDbContextAsync(async context =>
            {
                var game = await context.Games.FirstAsync(g => g.Slug == exclusiveSlug);
                return game.Id;
            });

            await _revenueContractAppService.SetContractAsync(exclusiveGameId, RevenueContractType.WebExclusive);

            var result = await _catalogAppService.GetGamesAsync(new GetGamesInput
            {
                Exclusivity = "WebExclusive",
                SkipCount = 0,
                MaxResultCount = 10
            });

            result.Items.ShouldContain(g => g.Slug == exclusiveSlug);
            result.Items.ShouldNotContain(g => g.Slug == normalSlug);
        }

        [Fact]
        public async Task Dado_FiltroMinRating_Quando_ListarJogos_Entao_RetornaJogosComRatingMinimo()
        {
            var highSlug = await SeedPublishedGameAsync("High Rating Game");
            await SeedPublishedGameAsync("Low Rating Game");

            var highGameId = await UsingDbContextAsync(async context =>
            {
                var game = await context.Games.FirstAsync(g => g.Slug == highSlug);
                return game.Id;
            });

            await _catalogAppService.VoteAsync(new GameVoteInput
            {
                GameId = highGameId,
                VoteType = GameVoteType.Like,
                DeviceId = "test-device"
            });

            var result = await _catalogAppService.GetGamesAsync(new GetGamesInput
            {
                MinRating = 4,
                SkipCount = 0,
                MaxResultCount = 10
            });

            result.Items.ShouldContain(g => g.Slug == highSlug);
        }

        [Fact]
        public async Task Dado_CategoriaComSeo_Quando_BuscarPorSlug_Entao_RetornaDescriptionEKeywords()
        {
            var categoryId = await SeedCategoryAsync("SEO Category");

            await UsingDbContextAsync(async context =>
            {
                var category = await context.Categories.FindAsync(categoryId);
                category.Description = "SEO description";
                category.Keywords = "seo, category";
                await context.SaveChangesAsync();
            });

            var result = await _catalogAppService.GetCategoryBySlugAsync("seo-category");

            result.ShouldNotBeNull();
            result.Description.ShouldBe("SEO description");
            result.Keywords.ShouldBe("seo, category");
        }

        [Fact]
        public async Task Dado_PlaytestDiscovery_Quando_CarregarMysteryTile_Entao_RetornaJogoMisterioso()
        {
            var slug = await SeedPublishedGameAsync("Mystery Game");
            var gameId = await UsingDbContextAsync(async context =>
            {
                var game = await context.Games.FirstAsync(g => g.Slug == slug);
                return game.Id;
            });

            await UsingDbContextAsync(async context =>
            {
                await context.PlaytestSessions.AddAsync(new PlaytestSession
                {
                    Id = Guid.NewGuid(),
                    TenantId = AbpSession.TenantId,
                    GameId = gameId,
                    RequestedByUserId = AbpSession.UserId ?? 1,
                    Status = PlaytestSessionStatus.Requested,
                    IsDiscovery = true,
                    DisplayProbability = 1.0,
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            });

            var tile = await _catalogAppService.GetMysteryTileAsync();

            tile.ShouldNotBeNull();
            tile.GameId.ShouldBe(gameId);
            tile.IsPlaytest.ShouldBeTrue();
            tile.Slug.ShouldBe(slug);
            tile.RecordingConsentPrompt.ShouldNotBeNullOrEmpty();
        }

        private async Task<string> SeedPublishedGameAsync(string title, long totalPlays = 0, List<Guid> categoryIds = null)
        {
            var input = new CreateGameDraftInput
            {
                Title = title,
                ShortDescription = "A test game",
                Description = "Description",
                AgeRating = "E",
                Orientation = "Both",
                SupportsDesktop = true,
                SupportsMobile = true,
                SupportsTablet = true,
                CategoryIds = categoryIds
            };

            var draft = await _developerGameAppService.CreateDraftAsync(input);

            await UsingDbContextAsync(async context =>
            {
                var game = await context.Games.FirstAsync(g => g.Id == draft.Id);
                game.Status = GameStatus.Published;
                game.TotalPlays = totalPlays;
            });

            return draft.Slug;
        }

        private async Task<Guid> SeedCategoryAsync(string name)
        {
            return await UsingDbContextAsync(async context =>
            {
                var category = new Category
                {
                    Id = Guid.NewGuid(),
                    TenantId = 1,
                    Name = name,
                    Slug = name.ToLowerInvariant().Replace(" ", "-"),
                    SortOrder = 0,
                    IsActive = true
                };
                await context.Categories.AddAsync(category);
                await context.SaveChangesAsync();
                return category.Id;
            });
        }
    }
}
