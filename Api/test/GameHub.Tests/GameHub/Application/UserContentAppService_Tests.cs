using System;
using System.Threading.Tasks;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Exceptions;
using GameHub.Moderation;
using GameHub.Moderation.Dto;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class UserContentAppService_Tests : GameHubTestBase
    {
        private readonly IUserContentAppService _userContentAppService;

        public UserContentAppService_Tests()
        {
            _userContentAppService = Resolve<IUserContentAppService>();
        }

        [Fact]
        public async Task Dado_TextoAprovado_Quando_Submeter_Entao_DeveEstarAprovado()
        {
            var gameId = await CriarJogoAsync();

            var result = await _userContentAppService.SubmitAsync(new SubmitUserContentInput
            {
                GameId = gameId,
                ContentType = UserContentType.Comment,
                Text = "Adorei o jogo!"
            });

            result.IsApproved.ShouldBeTrue();
            result.RequiresModeration.ShouldBeFalse();
        }

        [Fact]
        public async Task Dado_TextoComPalavrao_Quando_Submeter_Entao_DeveMarcarParaModeracao()
        {
            var gameId = await CriarJogoAsync();

            var result = await _userContentAppService.SubmitAsync(new SubmitUserContentInput
            {
                GameId = gameId,
                ContentType = UserContentType.Comment,
                Text = "Esse jogo é uma merda"
            });

            result.IsApproved.ShouldBeFalse();
            result.RequiresModeration.ShouldBeTrue();
        }

        [Fact]
        public async Task Dado_ConteudoModerado_Quando_Aprovar_Entao_DeveEstarAprovado()
        {
            var gameId = await CriarJogoAsync();

            var content = await _userContentAppService.SubmitAsync(new SubmitUserContentInput
            {
                GameId = gameId,
                ContentType = UserContentType.Comment,
                Text = "Texto suspeito"
            });

            content.IsApproved = false;
            content.RequiresModeration = true;

            var result = await _userContentAppService.ModerateAsync(new ModerateUserContentInput
            {
                ContentId = content.Id,
                IsApproved = true,
                Reason = "Aprovado após revisão"
            });

            result.IsApproved.ShouldBeTrue();
            result.RequiresModeration.ShouldBeFalse();
        }

        [Fact]
        public async Task Dado_MuitasSubmissoes_Quando_ExcederLimite_Entao_RejeitaComRateLimited()
        {
            var gameId = await CriarJogoAsync();

            for (var index = 0; index < 10; index++)
            {
                await _userContentAppService.SubmitAsync(new SubmitUserContentInput
                {
                    GameId = gameId,
                    ContentType = UserContentType.Comment,
                    Text = $"Comment {index}"
                });
            }

            var exception = await Should.ThrowAsync<GameHubException>(() =>
                _userContentAppService.SubmitAsync(new SubmitUserContentInput
                {
                    GameId = gameId,
                    ContentType = UserContentType.Comment,
                    Text = "One too many"
                }));

            exception.ErrorCode.ShouldBe(GameHubErrorCodes.RateLimited);
        }

        [Fact]
        public async Task Dado_MesmoClientRequestId_Quando_SubmeterDuasVezes_Entao_RetornaOMesmoConteudo()
        {
            var gameId = await CriarJogoAsync();

            var first = await _userContentAppService.SubmitAsync(new SubmitUserContentInput
            {
                GameId = gameId,
                ContentType = UserContentType.Comment,
                Text = "Ótimo jogo!",
                ClientRequestId = "req-123"
            });

            var second = await _userContentAppService.SubmitAsync(new SubmitUserContentInput
            {
                GameId = gameId,
                ContentType = UserContentType.Comment,
                Text = "Texto diferente",
                ClientRequestId = "req-123"
            });

            first.Id.ShouldBe(second.Id);
        }

        [Fact]
        public async Task Dado_JogoInexistente_Quando_Submeter_Entao_RetornaInvalidContext()
        {
            var exception = await Should.ThrowAsync<GameHubException>(() =>
                _userContentAppService.SubmitAsync(new SubmitUserContentInput
                {
                    GameId = Guid.NewGuid(),
                    ContentType = UserContentType.Comment,
                    Text = "Comentário"
                }));

            exception.ErrorCode.ShouldBe(GameHubErrorCodes.InvalidContext);
        }

        private async Task<Guid> CriarJogoAsync()
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

                await context.Games.AddAsync(new Game(gameId, "UserContent Test", "usercontent-test", "Test", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Published
                });
            });

            return gameId;
        }
    }
}
