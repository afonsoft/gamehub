using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Abp.Domain.Repositories;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Developer;
using GameHub.Developer.Dto;
using GameHub.Moderation;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class DeveloperGameAppService_Tests : GameHubTestBase
    {
        private readonly IDeveloperGameAppService _developerGameAppService;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<ModerationReview, Guid> _reviewRepository;

        public DeveloperGameAppService_Tests()
        {
            _developerGameAppService = Resolve<IDeveloperGameAppService>();
            _gameRepository = Resolve<IRepository<Game, Guid>>();
            _reviewRepository = Resolve<IRepository<ModerationReview, Guid>>();
        }

        [Fact]
        public async Task Dado_DraftValido_Quando_CriarRascunho_Entao_GeraSlugUnicoESalvaCategoriasETags()
        {
            var categoryId = await SeedCategoryAsync("Action");
            var tagId = await SeedTagAsync("2d");

            var input = new CreateGameDraftInput
            {
                Title = "Unique Game",
                ShortDescription = "A short description",
                Description = "A longer description",
                AgeRating = "E",
                Orientation = "Both",
                SupportsDesktop = true,
                SupportsMobile = true,
                SupportsTablet = true,
                CategoryIds = new List<Guid> { categoryId },
                TagIds = new List<Guid> { tagId }
            };

            var result = await _developerGameAppService.CreateDraftAsync(input);

            result.ShouldNotBeNull();
            result.Slug.ShouldBe("unique-game");

            var game = await UsingDbContextAsync(async context => await context.Games
                .Include(g => g.GameCategories)
                .Include(g => g.GameTags)
                .FirstOrDefaultAsync(g => g.Id == result.Id));
            game.ShouldNotBeNull();
            game.GameCategories.Count.ShouldBe(1);
            game.GameTags.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Dado_TituloDuplicado_Quando_CriarRascunho_Entao_GeraSlugComSufixo()
        {
            var input = new CreateGameDraftInput
            {
                Title = "Duplicate Title",
                ShortDescription = "First duplicate title",
                AgeRating = "E",
                Orientation = "Landscape"
            };

            var first = await _developerGameAppService.CreateDraftAsync(input);
            first.Slug.ShouldBe("duplicate-title");

            var second = await _developerGameAppService.CreateDraftAsync(input);
            second.Slug.ShouldBe("duplicate-title-2");
        }

        [Fact]
        public async Task Dado_JogoComBuild_Quando_SubmeterParaRevisao_Entao_CriaModerationReviewPending()
        {
            var gameId = await SeedGameWithBuildAsync("Review Game");

            await _developerGameAppService.SubmitForReviewAsync(new SubmitGameForReviewInput { GameId = gameId, Notes = "" });

            var review = await _reviewRepository.FirstOrDefaultAsync(r => r.GameId == gameId);
            review.ShouldNotBeNull();
            review.Status.ShouldBe(ModerationReviewStatus.Pending);

            var game = await _gameRepository.GetAsync(gameId);
            game.Status.ShouldBe(GameStatus.InReview);
        }

        private async Task<Guid> SeedGameWithBuildAsync(string title)
        {
            var game = await _developerGameAppService.CreateDraftAsync(new CreateGameDraftInput
            {
                Title = title,
                ShortDescription = "For review",
                AgeRating = "E",
                Orientation = "Both"
            });

            var buildId = Guid.NewGuid();
            var build = new GameBuild(buildId, game.Id, "1.0.1", 1, "/uploads/test.zip", 100, "hash")
            {
                TenantId = AbpSession.TenantId,
                Status = GameBuildStatus.Validated
            };

            await UsingDbContextAsync(async context =>
            {
                await context.GameBuilds.AddAsync(build);
                await context.SaveChangesAsync();
            });

            return game.Id;
        }

        private async Task<Guid> SeedCategoryAsync(string name)
        {
            return await UsingDbContextAsync(async context =>
            {
                var category = new Category
                {
                    Id = Guid.NewGuid(),
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

        private async Task<Guid> SeedTagAsync(string name)
        {
            return await UsingDbContextAsync(async context =>
            {
                var tag = new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Slug = name.ToLowerInvariant().Replace(" ", "-")
                };
                await context.Tags.AddAsync(tag);
                await context.SaveChangesAsync();
                return tag.Id;
            });
        }
    }
}
