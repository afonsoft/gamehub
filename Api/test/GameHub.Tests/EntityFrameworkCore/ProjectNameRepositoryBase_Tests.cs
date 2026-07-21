using GameHub.EntityFrameworkCore.Repositories;
using Shouldly;
using Xunit;

namespace GameHub.Tests.EntityFrameworkCore
{
    public class ProjectNameRepositoryBase_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_ProjectNameRepositoryBase_Quando_VerificarTipo_Entao_DeveSerClasseAbstrata()
        {
            var type = typeof(ProjectNameRepositoryBase<,>);
            type.ShouldNotBeNull();
            type.IsAbstract.ShouldBeTrue();
        }
    }
}
