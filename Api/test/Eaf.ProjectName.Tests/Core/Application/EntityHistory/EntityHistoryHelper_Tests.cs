using Eaf.ProjectName.EntityHistory;
using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Core.Application.EntityHistory
{
    public class EntityHistoryHelper_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_EntityHistoryHelper_Quando_VerificarTipo_Entao_DeveSerClasseEstatica()
        {
            var type = typeof(EntityHistoryHelper);
            type.ShouldNotBeNull();
            type.IsAbstract.ShouldBeTrue();
            type.IsSealed.ShouldBeTrue();
        }
    }
}
