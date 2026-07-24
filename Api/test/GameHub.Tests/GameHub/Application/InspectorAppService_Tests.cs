using System;
using System.Linq;
using System.Threading.Tasks;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Inspector;
using GameHub.Inspector.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class InspectorAppService_Tests : GameHubTestBase
    {
        private readonly IInspectorAppService _inspectorAppService;

        public InspectorAppService_Tests()
        {
            _inspectorAppService = Resolve<IInspectorAppService>();
        }

        [Fact]
        public async Task Dado_SessaoIniciada_Quando_LogarEventos_Entao_DeveRetornarSessaoComEventos()
        {
            var (gameId, _) = await CriarJogoAsync();

            var session = await _inspectorAppService.StartSessionAsync(new StartInspectorSessionInput
            {
                GameId = gameId,
                DevicePreset = "desktop",
                Resolution = "1024x768"
            });

            await _inspectorAppService.LogSdkEventAsync(new LogSdkEventInput
            {
                SessionId = session.Id,
                EventType = "gameLoadingStarted",
                SequenceNumber = 1
            });

            var detail = await _inspectorAppService.GetSessionAsync(session.Id);

            detail.ShouldNotBeNull();
            detail.Events.Count.ShouldBe(1);
            detail.Events[0].EventType.ShouldBe("gameLoadingStarted");
        }

        [Fact]
        public async Task Dado_GameplayAntesDoLoadingFinished_Quando_Validar_Entao_DeveGerarWarningCritico()
        {
            var (gameId, _) = await CriarJogoAsync();

            var session = await _inspectorAppService.StartSessionAsync(new StartInspectorSessionInput
            {
                GameId = gameId
            });

            await _inspectorAppService.LogSdkEventAsync(new LogSdkEventInput
            {
                SessionId = session.Id,
                EventType = "gameplayStart",
                SequenceNumber = 1
            });

            var warnings = await _inspectorAppService.ValidateSessionAsync(session.Id);

            warnings.ShouldContain(w => w.Message.Contains("gameplayStart", StringComparison.OrdinalIgnoreCase) && w.Severity == "Critical");
        }

        [Fact]
        public async Task Dado_EventoDuplicado_Quando_Validar_Entao_DeveGerarWarning()
        {
            var (gameId, _) = await CriarJogoAsync();

            var session = await _inspectorAppService.StartSessionAsync(new StartInspectorSessionInput
            {
                GameId = gameId
            });

            await _inspectorAppService.LogSdkEventAsync(new LogSdkEventInput
            {
                SessionId = session.Id,
                EventType = "gameLoadingStarted",
                SequenceNumber = 1
            });
            await _inspectorAppService.LogSdkEventAsync(new LogSdkEventInput
            {
                SessionId = session.Id,
                EventType = "gameLoadingStarted",
                SequenceNumber = 2
            });

            var warnings = await _inspectorAppService.ValidateSessionAsync(session.Id);

            warnings.ShouldContain(w => w.Category == "Duplicate");
        }

        [Fact]
        public async Task Dado_ChecklistRespondido_Quando_ObterCompletion_Entao_DeveRetornarPercentualCorreto()
        {
            var (gameId, _) = await CriarJogoAsync();

            var session = await _inspectorAppService.StartSessionAsync(new StartInspectorSessionInput
            {
                GameId = gameId
            });

            await _inspectorAppService.SaveChecklistAnswerAsync(new SaveChecklistAnswerInput
            {
                SessionId = session.Id,
                QuestionId = "indexHtml",
                Answer = "pass"
            });

            await _inspectorAppService.SaveChecklistAnswerAsync(new SaveChecklistAnswerInput
            {
                SessionId = session.Id,
                QuestionId = "viewport",
                Answer = "fail"
            });

            var completion = await _inspectorAppService.GetChecklistCompletionAsync(session.Id);

            completion.ShouldNotBeNull();
            completion.TotalQuestions.ShouldBe(8);
            completion.AnsweredQuestions.ShouldBe(2);
            completion.CompletionPercentage.ShouldBe(25);

            var detail = await _inspectorAppService.GetSessionAsync(session.Id);
            detail.ChecklistAnswers.Count.ShouldBe(2);
        }

        private async Task<(Guid gameId, Guid buildId)> CriarJogoAsync()
        {
            var userId = AbpSession.UserId.Value;
            var gameId = Guid.NewGuid();
            var profileId = Guid.NewGuid();

            await UsingDbContextAsync(async context =>
            {
                await context.DeveloperProfiles.AddAsync(new DeveloperProfile
                {
                    Id = profileId,
                    TenantId = AbpSession.TenantId,
                    UserId = userId,
                    DisplayName = "Dev User",
                    Status = DeveloperProfileStatus.Active
                });

                await context.Games.AddAsync(new Game(gameId, "Inspector Test", "inspector-test", "Test", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Draft
                });
            });

            return (gameId, Guid.Empty);
        }
    }
}
