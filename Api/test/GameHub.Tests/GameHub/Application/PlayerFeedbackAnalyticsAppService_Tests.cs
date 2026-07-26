using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Moderation;
using GameHub.Moderation.Dto;
using GameHub.Player;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class PlayerFeedbackAnalyticsAppService_Tests : GameHubTestBase
    {
        private readonly IPlayerFeedbackAnalyticsAppService _feedbackService;
        private readonly IUserContentAppService _contentAppService;

        public PlayerFeedbackAnalyticsAppService_Tests()
        {
            _feedbackService = LocalIocManager.Resolve<IPlayerFeedbackAnalyticsAppService>();
            _contentAppService = LocalIocManager.Resolve<IUserContentAppService>();
        }

        [Fact]
        public async Task Dado_AvaliacoesAprovadas_Quando_Resumo_Entao_RetornaMediaEDistribuicao()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("Feedback Game", "feedback-game");

            await _contentAppService.SubmitAsync(new SubmitUserContentInput
            {
                GameId = gameId,
                ContentType = UserContentType.Review,
                Text = "Great game",
                Rating = 5
            });

            await _contentAppService.SubmitAsync(new SubmitUserContentInput
            {
                GameId = gameId,
                ContentType = UserContentType.Review,
                Text = "Good but short",
                Rating = 4
            });

            var summary = await _feedbackService.GetFeedbackSummaryAsync(gameId);

            summary.ShouldNotBeNull();
            summary.TotalReviews.ShouldBe(2);
            summary.AverageRating.ShouldBe(4.5, tolerance: 0.01);
            summary.Distribution.ShouldContainKey(5);
            summary.Distribution.ShouldContainKey(4);
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

                await context.Games.AddAsync(new Game(gameId, title, slug, "Test", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Draft
                });
            });

            return gameId;
        }
    }
}
