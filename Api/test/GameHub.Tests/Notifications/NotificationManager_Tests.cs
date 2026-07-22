using Abp.Notifications;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Notifications
{
    /// <summary>
    /// Testes para gerenciamento de notificações seguindo o padrão BDD (Given/When/Then) em português
    /// </summary>
    public class NotificationManager_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_SistemaInicializado_Quando_CriarNotificationData_Entao_DeveCriarComSucesso()
        {
            // Dado (Given)
            var notificationData = new NotificationData();
            notificationData["message"] = "Mensagem de teste";

            // Quando (When)
            notificationData.ShouldNotBeNull();

            // Então (Then)
            notificationData["message"].ShouldBe("Mensagem de teste");
        }

        [Fact]
        public void Dado_SistemaInicializado_Quando_CriarNotificationDataComDadosExtras_Entao_DeveIncluirDadosExtras()
        {
            // Dado (Given)
            var notificationData = new NotificationData();
            notificationData["message"] = "Mensagem de teste";
            notificationData["ChaveExtra"] = "ValorExtra";

            // Quando (When)
            var valorExtra = notificationData["ChaveExtra"];

            // Então (Then)
            valorExtra.ShouldBe("ValorExtra");
        }

        [Fact]
        public void Dado_SistemaInicializado_Quando_CriarNotificationDataSemDados_Entao_DeveCriarComDadosVazios()
        {
            // Dado (Given)
            var notificationData = new NotificationData();

            // Quando (When)
            var properties = notificationData.Properties;

            // Então (Then)
            properties.ShouldNotBeNull();
            properties.Count.ShouldBe(0);
        }
    }
}
