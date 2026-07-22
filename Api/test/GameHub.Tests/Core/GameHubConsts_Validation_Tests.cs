using Shouldly;
using Xunit;

namespace GameHub.Tests.Core
{
    public class GameHubConsts_Validation_Tests
    {
        [Fact]
        public void Dado_GameHubConsts_Quando_LocalizationSourceName_Entao_DeveSerGameHub()
        {
            GameHubConsts.LocalizationSourceName.ShouldBe("GameHub");
        }

        [Fact]
        public void Dado_GameHubConsts_Quando_ConnectionStringName_Entao_DeveSerDefault()
        {
            GameHubConsts.ConnectionStringName.ShouldBe("Default");
        }

        [Fact]
        public void Dado_GameHubConsts_Quando_DefaultCorsPolicyName_Entao_DeveSerGameHubCorsPolicy()
        {
            GameHubConsts.DefaultCorsPolicyName.ShouldBe("GameHubCorsPolicy");
        }

        [Fact]
        public void Dado_GameHubConsts_Quando_MultiTenancyEnabled_Entao_DeveSerTrue()
        {
            GameHubConsts.MultiTenancyEnabled.ShouldBeTrue();
        }
    }
}
