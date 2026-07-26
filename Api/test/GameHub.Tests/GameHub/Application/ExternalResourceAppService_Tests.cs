using System;
using System.Threading.Tasks;
using GameHub.Builds;
using GameHub.Builds.Dto;
using GameHub.Catalog;
using GameHub.Developers;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class ExternalResourceAppService_Tests : GameHubTestBase
    {
        private readonly IExternalResourceAppService _externalResourceAppService;

        public ExternalResourceAppService_Tests()
        {
            _externalResourceAppService = LocalIocManager.Resolve<IExternalResourceAppService>();
        }

        [Fact]
        public async Task Dado_DominioPendente_Quando_Aprovar_Entao_StatusAprovado()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("External Resource Game", "external-resource-game");

            var request = await _externalResourceAppService.RequestExemptionAsync(new RequestExternalResourceExemptionInput
            {
                GameId = gameId,
                Domain = "analytics.example.com",
                ProviderName = "Example Analytics"
            });

            request.Status.ShouldBe("Pending");

            var approved = await _externalResourceAppService.ReviewAsync(new ReviewExternalResourceExemptionInput
            {
                Id = request.Id,
                IsApproved = true
            });

            approved.Status.ShouldBe("Approved");

            var list = await _externalResourceAppService.GetByGameAsync(gameId);
            list.Count.ShouldBe(1);
            list[0].Status.ShouldBe("Approved");
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
