using GameHub.EntityFrameworkCore.Repositories;
using Shouldly;
using Xunit;

namespace GameHub.Tests.EntityFrameworkCore
{
    public class GameHubRepositoryBase_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_GameHubRepositoryBase_Quando_VerificarTipo_Entao_DeveSerClasseAbstrata()
        {
            var type = typeof(GameHubRepositoryBase<,>);
            type.ShouldNotBeNull();
            type.IsAbstract.ShouldBeTrue();
        }
    }
}
