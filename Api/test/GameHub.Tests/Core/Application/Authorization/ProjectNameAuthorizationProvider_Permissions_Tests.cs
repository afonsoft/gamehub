using GameHub.Authorization;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core.Application.Authorization
{
    public class ProjectNameAuthorizationProvider_Permissions_Tests
    {
        [Fact]
        public void Dado_Permissoes_Quando_PagesAirplanes_Entao_DeveSerPagesAirplanes()
        {
            ProjectNamePermissions.Pages_Airplanes.ShouldBe("Pages.Airplanes");
        }

        [Fact]
        public void Dado_Permissoes_Quando_PagesAirplanesCreate_Entao_DeveSerPagesAirplanesCreate()
        {
            ProjectNamePermissions.Pages_Airplanes_Create.ShouldBe("Pages.Airplanes.Create");
        }

        [Fact]
        public void Dado_Permissoes_Quando_PagesAirplanesEdit_Entao_DeveSerPagesAirplanesEdit()
        {
            ProjectNamePermissions.Pages_Airplanes_Edit.ShouldBe("Pages.Airplanes.Edit");
        }

        [Fact]
        public void Dado_Permissoes_Quando_PagesAirplanesDelete_Entao_DeveSerPagesAirplanesDelete()
        {
            ProjectNamePermissions.Pages_Airplanes_Delete.ShouldBe("Pages.Airplanes.Delete");
        }

        [Fact]
        public void Dado_Permissoes_Quando_Verificar_Entao_DevemSeguirConvencaoHierarquica()
        {
            // Então (Then) - Todas as permissões filhas devem começar com a permissão pai
            ProjectNamePermissions.Pages_Airplanes_Create.ShouldStartWith("Pages.Airplanes");
            ProjectNamePermissions.Pages_Airplanes_Edit.ShouldStartWith("Pages.Airplanes");
            ProjectNamePermissions.Pages_Airplanes_Delete.ShouldStartWith("Pages.Airplanes");
        }
    }
}
