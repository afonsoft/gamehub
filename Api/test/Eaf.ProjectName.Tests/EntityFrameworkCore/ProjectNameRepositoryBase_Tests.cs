using Eaf.ProjectName.EntityFrameworkCore.Repositories;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.EntityFrameworkCore
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
