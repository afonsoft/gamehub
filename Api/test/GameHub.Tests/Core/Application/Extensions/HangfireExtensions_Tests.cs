using GameHub.Application.Extensions;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core.Application.Extensions
{
    public class HangfireExtensions_Tests : ProjectNameTestBase
    {
        [Fact]
        public void Dado_HangfireExtensions_Quando_VerificarTipo_Entao_DeveSerClasseEstatica()
        {
            var type = typeof(HangfireExtensions);
            type.ShouldNotBeNull();
            type.IsAbstract.ShouldBeTrue();
            type.IsSealed.ShouldBeTrue();
        }
    }
}
