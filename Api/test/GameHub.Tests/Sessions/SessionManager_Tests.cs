using Abp.Runtime.Session;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Sessions
{
    /// <summary>
    /// Testes para gerenciamento de sessões seguindo o padrão BDD (Given/When/Then) em português
    /// </summary>
    public class SessionManager_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_SistemaInicializado_Quando_ObterUserId_Entao_DeveRetornarIdValido()
        {
            // Dado (Given)
            LoginAsDefaultTenantAdmin();

            // Quando (When)
            var userId = AbpSession.UserId;

            // Então (Then)
            userId.ShouldNotBeNull();
            userId.Value.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Dado_SistemaInicializado_Quando_ObterTenantId_Entao_DeveRetornarIdValido()
        {
            // Dado (Given)
            LoginAsDefaultTenantAdmin();

            // Quando (When)
            var tenantId = AbpSession.TenantId;

            // Então (Then)
            tenantId.ShouldNotBeNull();
            tenantId.Value.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Dado_LogadoComoHost_Quando_ObterTenantId_Entao_DeveRetornarNulo()
        {
            // Dado (Given)
            LoginAsHostAdmin();

            // Quando (When)
            var tenantId = AbpSession.TenantId;

            // Então (Then)
            tenantId.ShouldBeNull();
        }

        [Fact]
        public void Dado_SistemaInicializado_Quando_ObterMultiTenancySide_Entao_DeveRetornarLadoCorreto()
        {
            // Dado (Given)
            LoginAsDefaultTenantAdmin();

            // Quando (When)
            var multiTenancySide = AbpSession.MultiTenancySide;

            // Então (Then)
            multiTenancySide.ShouldBe(Abp.MultiTenancy.MultiTenancySides.Tenant);
        }
    }
}
