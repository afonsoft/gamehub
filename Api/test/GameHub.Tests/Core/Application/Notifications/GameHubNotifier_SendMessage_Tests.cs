using Abp;
using Abp.Notifications;
using GameHub.Notifications;
using NSubstitute;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace GameHub.Tests.Core.Application.Notifications
{
    public class GameHubNotifier_SendMessage_Tests : GameHubTestBase
    {
        private readonly IGameHubNotifier _notifier;

        public GameHubNotifier_SendMessage_Tests()
        {
            _notifier = LocalIocManager.Resolve<IGameHubNotifier>();
        }

        [Fact]
        public async Task Dado_Notifier_Quando_EnviarMensagem_Entao_DeveExecutarSemExcecao()
        {
            var user = new UserIdentifier(AbpSession.TenantId, AbpSession.UserId.Value);

            await Should.NotThrowAsync(async () => await _notifier.SendMessageAsync(user, "Mensagem de teste"));
        }

        [Fact]
        public async Task Dado_Notifier_Quando_EnviarMensagemComSeveridade_Entao_DeveExecutarSemExcecao()
        {
            var user = new UserIdentifier(AbpSession.TenantId, AbpSession.UserId.Value);

            await Should.NotThrowAsync(async () => await _notifier.SendMessageAsync(user, "Mensagem crítica", NotificationSeverity.Error));
        }

        [Fact]
        public async Task Dado_Notifier_Quando_EnviarMensagemWarning_Entao_DeveExecutarSemExcecao()
        {
            var user = new UserIdentifier(AbpSession.TenantId, AbpSession.UserId.Value);

            await Should.NotThrowAsync(async () => await _notifier.SendMessageAsync(user, "Aviso importante", NotificationSeverity.Warn));
        }
    }
}
