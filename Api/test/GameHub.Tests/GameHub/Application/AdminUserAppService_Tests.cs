using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using GameHub.Admin;
using GameHub.Admin.Dto;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class AdminUserAppService_Tests : GameHubTestBase
    {
        private readonly IAdminUserAppService _adminUserAppService;

        public AdminUserAppService_Tests()
        {
            _adminUserAppService = Resolve<IAdminUserAppService>();
        }

        [Fact]
        public async Task Dado_UsuariosCadastrados_Quando_Listar_Entao_RetornaUsuariosComFlagDeveloper()
        {
            LoginAsHostAdmin();

            var result = await _adminUserAppService.GetAllAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 10 });

            result.ShouldNotBeNull();
            result.TotalCount.ShouldBeGreaterThan(0);
            result.Items.Count.ShouldBeGreaterThan(0);

            var admin = result.Items.FirstOrDefault(u => u.UserName == "admin");
            admin.ShouldNotBeNull();
            admin.IsActive.ShouldBeTrue();
            admin.IsDeveloper.ShouldBeFalse();
        }
    }
}
