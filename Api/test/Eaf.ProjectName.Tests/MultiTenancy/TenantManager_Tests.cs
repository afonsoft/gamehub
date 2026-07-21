using Abp.MultiTenancy;
using Eaf.Middleware.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Eaf.ProjectName.Tests.MultiTenancy
{
    /// <summary>
    /// Testes para gerenciamento de multi-tenancy seguindo o padrão BDD (Given/When/Then) em português
    /// </summary>
    public class TenantManager_Tests : ProjectNameTestBase
    {
        [Fact]
        public async Task Dado_SistemaInicializado_Quando_ObterTenantPadrao_Entao_DeveRetornarTenantValido()
        {
            // Dado (Given)
            AbpSession.TenantId = null;

            // Quando (When)
            var tenant = await UsingDbContextAsync(context => context.Tenants.FirstOrDefaultAsync(t => t.TenancyName == AbpTenantBase.DefaultTenantName));

            // Então (Then)
            tenant.ShouldNotBeNull();
            tenant.TenancyName.ShouldBe(AbpTenantBase.DefaultTenantName);
        }

        [Fact]
        public async Task Dado_SistemaInicializado_Quando_CriarNovoTenant_Entao_DeveCriarTenantComSucesso()
        {
            // Dado (Given)
            AbpSession.TenantId = null;
            var tenantName = "NovoTenant";

            // Quando (When)
            var tenant = new Tenant(tenantName, tenantName);
            await UsingDbContextAsync(async context =>
            {
                await context.Tenants.AddAsync(tenant);
                await context.SaveChangesAsync();
            });

            // Então (Then)
            var tenantCriado = await UsingDbContextAsync(context => context.Tenants.FirstOrDefaultAsync(t => t.TenancyName == tenantName));
            tenantCriado.ShouldNotBeNull();
            tenantCriado.TenancyName.ShouldBe(tenantName);
        }

        [Fact]
        public void Dado_SistemaInicializado_Quando_TrocarTenantId_Entao_DeveTrocarContextoCorretamente()
        {
            // Dado (Given)
            var tenantId = 1;

            // Quando (When)
            using (UsingTenantId(tenantId))
            {
                var currentTenantId = AbpSession.TenantId;

                // Então (Then)
                currentTenantId.ShouldBe(tenantId);
            }
        }

        [Fact]
        public void Dado_SistemaInicializado_Quando_TrocarTenantIdParaNulo_Entao_DeveTrocarParaHost()
        {
            // Dado (Given)
            AbpSession.TenantId = 1;

            // Quando (When)
            using (UsingTenantId(null))
            {
                var currentTenantId = AbpSession.TenantId;

                // Então (Then)
                currentTenantId.ShouldBeNull();
            }
        }
    }
}
