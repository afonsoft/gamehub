using Abp.Domain.Entities.Auditing;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Entities
{
    public class FullAuditedEntity_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_FullAuditedEntity_Quando_VerificarTipo_Entao_DeveTerPropriedadesDeAuditoria()
        {
            var type = typeof(FullAuditedEntity);
            type.ShouldNotBeNull();
            type.IsAbstract.ShouldBeTrue();
        }
    }
}
