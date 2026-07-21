using Abp.Dependency;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.DependencyInjection
{
    /// <summary>
    /// Testes para injeção de dependência seguindo o padrão BDD (Given/When/Then) em português
    /// </summary>
    public class DependencyInjection_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_SistemaInicializado_Quando_ResolverServicoDoContainer_Entao_DeveRetornarInstanciaValida()
        {
            // Dado (Given)
            var serviceType = typeof(Abp.Runtime.Session.IAbpSession);

            // Quando (When)
            var service = LocalIocManager.Resolve(serviceType);

            // Então (Then)
            service.ShouldNotBeNull();
            service.ShouldBeAssignableTo(serviceType);
        }

        [Fact]
        public void Dado_SistemaInicializado_Quando_ResolverServicoComInterface_Entao_DeveRetornarInstanciaCorreta()
        {
            // Dado (Given)
            var session = LocalIocManager.Resolve<Abp.Runtime.Session.IAbpSession>();

            // Quando (When)
            var userId = session.UserId;

            // Então (Then)
            userId.ShouldNotBeNull();
            userId.Value.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Dado_SistemaInicializado_Quando_TentarResolverServicoNaoRegistrado_Entao_DeveLancarExcecao()
        {
            // Dado (Given)
            var serviceType = typeof(System.String);

            // Quando (When)
            var exception = Record.Exception(() => LocalIocManager.Resolve(serviceType));

            // Então (Then)
            exception.ShouldNotBeNull();
        }
    }
}
