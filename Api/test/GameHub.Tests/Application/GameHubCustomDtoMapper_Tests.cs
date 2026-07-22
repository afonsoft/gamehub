using Abp.ObjectMapping;
using GameHub.Catalog;
using GameHub.Catalog.Dto;
using Shouldly;
using System;
using Xunit;

namespace GameHub.Tests.Application
{
    public class GameHubCustomDtoMapper_Tests : GameHubTestBase
    {
        private readonly IObjectMapper _objectMapper;

        public GameHubCustomDtoMapper_Tests()
        {
            _objectMapper = LocalIocManager.Resolve<IObjectMapper>();
        }

        [Fact]
        public void Dado_MapperConfigurado_Quando_MapearGameParaGameCardDto_Entao_DeveMapearCorretamente()
        {
            var game = new Game(Guid.NewGuid(), "Game Title", "game-title", "Short description", Guid.NewGuid())
            {
                ThumbnailUrl = "https://cdn/game.png",
                TotalPlays = 42
            };

            var dto = _objectMapper.Map<GameCardDto>(game);

            dto.ShouldNotBeNull();
            dto.Title.ShouldBe("Game Title");
            dto.Slug.ShouldBe("game-title");
            dto.ShortDescription.ShouldBe("Short description");
            dto.ThumbnailUrl.ShouldBe("https://cdn/game.png");
            dto.TotalPlays.ShouldBe(42);
            dto.SupportsMobile.ShouldBeTrue();
            dto.SupportsDesktop.ShouldBeTrue();
        }

        [Fact]
        public void Dado_MapperConfigurado_Quando_MapearCategoryParaCategoryDto_Entao_DeveMapearCorretamente()
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Action",
                Slug = "action",
                SortOrder = 1,
                IsActive = true
            };

            var dto = _objectMapper.Map<CategoryDto>(category);

            dto.ShouldNotBeNull();
            dto.Id.ShouldBe(category.Id);
            dto.Name.ShouldBe("Action");
            dto.Slug.ShouldBe("action");
            dto.SortOrder.ShouldBe(1);
        }

        [Fact]
        public void Dado_MapperConfigurado_Quando_MapearGameSemDescricao_Entao_DevePreencherValoresPadrao()
        {
            var game = new Game(Guid.NewGuid(), "Minimal Game", "minimal-game", string.Empty, Guid.NewGuid());

            var dto = _objectMapper.Map<GameCardDto>(game);

            dto.ShouldNotBeNull();
            dto.Title.ShouldBe("Minimal Game");
            dto.ShortDescription.ShouldBeEmpty();
            dto.Categories.ShouldNotBeNull();
        }
    }
}
