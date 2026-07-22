using Abp.Domain.Repositories;
using GameHub.Catalog;
using Shouldly;
using System;
using Xunit;

namespace GameHub.Tests.Core
{
    public class GameHubDomainServiceBase_Localization_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_DomainService_Quando_Resolver_Entao_DeveEstarConfigurado()
        {
            var repository = LocalIocManager.Resolve<IRepository<Game, Guid>>();

            repository.ShouldNotBeNull();
        }

        [Fact]
        public void Dado_DomainServiceBase_Quando_VerificarTipo_Entao_DeveSerAbstrato()
        {
            var type = typeof(GameHubDomainServiceBase);

            type.IsAbstract.ShouldBeTrue();
        }

        [Fact]
        public void Dado_DomainServiceBase_Quando_VerificarHeranca_Entao_DeveHerdarDeDomainService()
        {
            typeof(GameHubDomainServiceBase)
                .BaseType.Name.ShouldBe("DomainService");
        }
    }
}
