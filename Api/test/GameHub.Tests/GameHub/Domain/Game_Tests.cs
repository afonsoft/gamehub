using System;
using GameHub;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Developers;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Domain
{
    public class Game_Tests
    {
        [Fact]
        public void Dado_NovoJogo_Quando_CriarDraft_Entao_StatusDeveSerDraft()
        {
            var game = new Game(Guid.NewGuid(), "Super Game", "super-game", "A cool game", Guid.NewGuid());

            game.Status.ShouldBe(GameStatus.Draft);
            game.TotalPlays.ShouldBe(0);
        }

        [Fact]
        public void Dado_JogoSemBuildAprovado_Quando_Publicar_Entao_DeveLancarExcecao()
        {
            var game = new Game(Guid.NewGuid(), "Super Game", "super-game", "A cool game", Guid.NewGuid());
            game.Status = GameStatus.InReview;

            Should.Throw<InvalidOperationException>(() => game.Publish());
        }

        [Fact]
        public void Dado_BuildAprovado_Quando_Publicar_Entao_StatusDeveSerPublished()
        {
            var game = new Game(Guid.NewGuid(), "Super Game", "super-game", "A cool game", Guid.NewGuid());
            var build = game.AddBuild(Guid.NewGuid(), "1.0.0", 1, "/uploads/1.zip", 1024, "abc");
            build.Approve();
            game.Status = GameStatus.InReview;

            game.Publish();

            game.Status.ShouldBe(GameStatus.Published);
            game.PublishedBuildId.ShouldNotBeNull();
        }
    }
}
