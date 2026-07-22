using GameHub.Authorization;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Core.Application.Authorization
{
    public class GameHubAuthorizationProvider_Permissions_Tests
    {
        [Fact]
        public void Dado_Permissoes_Quando_PagesAirplanes_Entao_DeveSerPagesAirplanes()
        {
            GameHubPermissions.Pages_Airplanes.ShouldBe("Pages.Airplanes");
        }

        [Fact]
        public void Dado_Permissoes_Quando_PagesAirplanesCreate_Entao_DeveSerPagesAirplanesCreate()
        {
            GameHubPermissions.Pages_Airplanes_Create.ShouldBe("Pages.Airplanes.Create");
        }

        [Fact]
        public void Dado_Permissoes_Quando_PagesAirplanesEdit_Entao_DeveSerPagesAirplanesEdit()
        {
            GameHubPermissions.Pages_Airplanes_Edit.ShouldBe("Pages.Airplanes.Edit");
        }

        [Fact]
        public void Dado_Permissoes_Quando_PagesAirplanesDelete_Entao_DeveSerPagesAirplanesDelete()
        {
            GameHubPermissions.Pages_Airplanes_Delete.ShouldBe("Pages.Airplanes.Delete");
        }

        [Fact]
        public void Dado_Permissoes_Quando_Verificar_Entao_DevemSeguirConvencaoHierarquica()
        {
            // Então (Then) - Todas as permissões filhas devem começar com a permissão pai
            GameHubPermissions.Pages_Airplanes_Create.ShouldStartWith("Pages.Airplanes");
            GameHubPermissions.Pages_Airplanes_Edit.ShouldStartWith("Pages.Airplanes");
            GameHubPermissions.Pages_Airplanes_Delete.ShouldStartWith("Pages.Airplanes");
        }
    }
}
