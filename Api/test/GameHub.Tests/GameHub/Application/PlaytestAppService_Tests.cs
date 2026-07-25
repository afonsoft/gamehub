using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub.Catalog;
using GameHub.Developer;
using GameHub.Developer.Dto;
using GameHub.Developers;
using GameHub.Playtesting;
using GameHub.Playtesting.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class PlaytestAppService_Tests : GameHubTestBase
    {
        private readonly IPlaytestAppService _playtestAppService;
        private readonly IDeveloperTeamAppService _developerTeamAppService;

        public PlaytestAppService_Tests()
        {
            _playtestAppService = Resolve<IPlaytestAppService>();
            _developerTeamAppService = Resolve<IDeveloperTeamAppService>();
        }

        [Fact]
        public async Task Dado_JogoValido_Quando_SolicitarPlaytest_Entao_SessaoFicaComoSolicitada()
        {
            var gameId = await SeedGameAndTeamAsync();

            var playtest = await _playtestAppService.RequestPlaytestAsync(new RequestPlaytestInput
            {
                GameId = gameId,
                Notes = "Please test mobile controls"
            });

            playtest.ShouldNotBeNull();
            playtest.GameId.ShouldBe(gameId);
            playtest.Status.ShouldBe(PlaytestSessionStatus.Requested.ToString());
            playtest.Notes.ShouldBe("Please test mobile controls");
        }

        [Fact]
        public async Task Dado_PlaytestsSolicitados_Quando_ConsultarPorJogo_Entao_RetornaLista()
        {
            var gameId = await SeedGameAndTeamAsync();

            await _playtestAppService.RequestPlaytestAsync(new RequestPlaytestInput { GameId = gameId });
            await _playtestAppService.RequestPlaytestAsync(new RequestPlaytestInput { GameId = gameId });

            var result = await _playtestAppService.GetPlaytestsByGameAsync(gameId);

            result.Items.Count.ShouldBe(2);
        }

        [Fact]
        public async Task Dado_PlaytestSolicitado_Quando_EnviarGravacao_Entao_SessaoEhCompletada()
        {
            var gameId = await SeedGameAndTeamAsync();

            var playtest = await _playtestAppService.RequestPlaytestAsync(new RequestPlaytestInput { GameId = gameId });

            var updated = await _playtestAppService.UploadRecordingAsync(new UploadPlaytestRecordingInput
            {
                PlaytestId = playtest.Id,
                RecordingUrl = "https://storage.gamehub.local/recordings/test.mp4"
            });

            updated.Status.ShouldBe(PlaytestSessionStatus.Completed.ToString());
            updated.RecordingUrl.ShouldBe("https://storage.gamehub.local/recordings/test.mp4");
            updated.CompletedAt.ShouldNotBeNull();
        }

        private async Task<Guid> SeedGameAndTeamAsync()
        {
            var team = await _developerTeamAppService.CreateTeamAsync(new CreateOrUpdateDeveloperTeamInput
            {
                Name = "Playtest Team",
                PrimaryContactEmail = "playtest@gamehub.local",
                Country = "BR"
            });

            var gameId = Guid.NewGuid();
            var profileId = Guid.NewGuid();

            await UsingDbContextAsync(async context =>
            {
                await context.DeveloperProfiles.AddAsync(new DeveloperProfile
                {
                    Id = profileId,
                    TenantId = AbpSession.TenantId,
                    UserId = AbpSession.UserId.Value,
                    DisplayName = "Dev User",
                    Status = DeveloperProfileStatus.Active
                });

                await context.Games.AddAsync(new Game(gameId, "Playtest Game", "playtest-game", "Test", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Draft
                });
            });

            return gameId;
        }
    }
}
