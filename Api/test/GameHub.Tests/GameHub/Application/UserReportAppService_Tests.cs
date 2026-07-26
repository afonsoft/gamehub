using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub.Catalog;
using GameHub.Moderation;
using GameHub.Moderation.Dto;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class UserReportAppService_Tests : GameHubTestBase
    {
        private readonly IUserReportAppService _userReportAppService;

        public UserReportAppService_Tests()
        {
            _userReportAppService = LocalIocManager.Resolve<IUserReportAppService>();
        }

        [Fact]
        public async Task Dado_MaisDeDezReports_Quando_SubmeterNoMesmoMinuto_Entao_RejeitaPorRateLimit()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync();

            for (var index = 0; index < 10; index++)
            {
                await _userReportAppService.SubmitAsync(new UserReportInput
                {
                    GameId = gameId,
                    Reason = $"reason-{index}"
                });
            }

            await Should.ThrowAsync<InvalidOperationException>(() =>
                _userReportAppService.SubmitAsync(new UserReportInput
                {
                    GameId = gameId,
                    Reason = "eleventh-report"
                }));
        }

        private async Task<Guid> SeedGameAsync()
        {
            var gameId = Guid.NewGuid();
            await UsingDbContextAsync(async context =>
            {
                await context.Games.AddAsync(new Game(
                    gameId,
                    "Report Test Game",
                    $"report-{gameId:N}",
                    "Report test game",
                    Guid.NewGuid())
                {
                    TenantId = AbpSession.TenantId
                });
                await context.SaveChangesAsync();
            });

            return gameId;
        }
    }
}
