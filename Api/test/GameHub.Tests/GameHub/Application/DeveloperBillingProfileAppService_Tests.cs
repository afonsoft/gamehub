using System;
using System.Threading.Tasks;
using GameHub.Developer;
using GameHub.Developer.Dto;
using Shouldly;
using Xunit;

using CreateTeamInput = GameHub.Developer.Dto.CreateOrUpdateDeveloperTeamInput;

namespace GameHub.Tests.GameHub.Application
{
    public class DeveloperBillingProfileAppService_Tests : GameHubTestBase
    {
        private readonly IDeveloperTeamAppService _developerTeamAppService;
        private readonly IDeveloperBillingProfileAppService _billingProfileAppService;

        public DeveloperBillingProfileAppService_Tests()
        {
            _developerTeamAppService = Resolve<IDeveloperTeamAppService>();
            _billingProfileAppService = Resolve<IDeveloperBillingProfileAppService>();
        }

        [Fact]
        public async Task Dado_EquipeCriada_Quando_SalvarDadosFaturamento_Entao_FicaPendenteDeAprovacao()
        {
            var team = await _developerTeamAppService.CreateTeamAsync(new CreateTeamInput
            {
                Name = "Billing Team",
                PrimaryContactEmail = "billing@gamehub.local",
                Country = "BR"
            });

            var saved = await _billingProfileAppService.SaveAsync(new SaveDeveloperBillingProfileInput
            {
                TeamId = team.Id,
                TaxId = "123456789",
                Address = "Rua Teste, 123",
                PaymentMethodPlaceholder = "**** **** **** 1234"
            });

            saved.ShouldNotBeNull();
            saved.TeamId.ShouldBe(team.Id);
            saved.TaxId.ShouldBe("123456789");
            saved.IsPendingReview.ShouldBeTrue();
            saved.IsApproved.ShouldBeFalse();
        }

        [Fact]
        public async Task Dado_PerfilSalvo_Quando_ConsultarPorEquipe_Entao_RetornaDados()
        {
            var team = await _developerTeamAppService.CreateTeamAsync(new CreateTeamInput
            {
                Name = "Billing Team",
                PrimaryContactEmail = "billing@gamehub.local",
                Country = "BR"
            });

            await _billingProfileAppService.SaveAsync(new SaveDeveloperBillingProfileInput
            {
                TeamId = team.Id,
                TaxId = "987654321",
                Address = "Av Teste, 456",
                PaymentMethodPlaceholder = "PIX"
            });

            var profile = await _billingProfileAppService.GetByTeamAsync(team.Id);

            profile.ShouldNotBeNull();
            profile.TaxId.ShouldBe("987654321");
            profile.IsPendingReview.ShouldBeTrue();
        }
    }
}
