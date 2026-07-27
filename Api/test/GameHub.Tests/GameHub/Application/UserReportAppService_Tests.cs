using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub.Catalog;
using GameHub.Exceptions;
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
        public async Task Dado_MaisDeDezReports_Quando_SubmeterNoMesmoMinuto_Entao_RejeitaComRateLimited()
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

            var exception = await Should.ThrowAsync<GameHubException>(() =>
                _userReportAppService.SubmitAsync(new UserReportInput
                {
                    GameId = gameId,
                    Reason = "eleventh-report"
                }));

            exception.ErrorCode.ShouldBe(GameHubErrorCodes.RateLimited);
        }

        [Fact]
        public async Task Dado_MesmoClientRequestId_Quando_SubmeterDuasVezes_Entao_RetornaOMesmoReport()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync();

            var first = await _userReportAppService.SubmitAsync(new UserReportInput
            {
                GameId = gameId,
                Reason = "cheating",
                ClientRequestId = "report-123"
            });

            var second = await _userReportAppService.SubmitAsync(new UserReportInput
            {
                GameId = gameId,
                Reason = "different",
                ClientRequestId = "report-123"
            });

            first.ReportId.ShouldBe(second.ReportId);
        }

        [Fact]
        public async Task Dado_JogoInexistente_Quando_Submeter_Entao_RetornaInvalidContext()
        {
            LoginAsDefaultTenantAdmin();

            var exception = await Should.ThrowAsync<GameHubException>(() =>
                _userReportAppService.SubmitAsync(new UserReportInput
                {
                    GameId = Guid.NewGuid(),
                    Reason = "invalid"
                }));

            exception.ErrorCode.ShouldBe(GameHubErrorCodes.InvalidContext);
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
