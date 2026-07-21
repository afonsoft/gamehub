using Shouldly;
using Xunit;

namespace Eaf.ProjectName.Tests.Core
{
    public class ProjectNameConsts_Validation_Tests
    {
        [Fact]
        public void Dado_ProjectNameConsts_Quando_LocalizationSourceName_Entao_DeveSerProjectName()
        {
            ProjectNameConsts.LocalizationSourceName.ShouldBe("ProjectName");
        }

        [Fact]
        public void Dado_ProjectNameConsts_Quando_ConnectionStringName_Entao_DeveSerDefault()
        {
            ProjectNameConsts.ConnectionStringName.ShouldBe("Default");
        }

        [Fact]
        public void Dado_ProjectNameConsts_Quando_DefaultCorsPolicyName_Entao_DeveSerProjectNameCorsPolicy()
        {
            ProjectNameConsts.DefaultCorsPolicyName.ShouldBe("ProjectNameCorsPolicy");
        }

        [Fact]
        public void Dado_ProjectNameConsts_Quando_MultiTenancyEnabled_Entao_DeveSerTrue()
        {
            ProjectNameConsts.MultiTenancyEnabled.ShouldBeTrue();
        }
    }
}
