using Abp.Domain.Entities;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Entities
{
    public class IMustHaveTenant_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_IMustHaveTenant_Quando_VerificarTipo_Entao_DeveSerInterface()
        {
            var type = typeof(IMustHaveTenant);
            type.ShouldNotBeNull();
            type.IsInterface.ShouldBeTrue();
        }
    }
}
