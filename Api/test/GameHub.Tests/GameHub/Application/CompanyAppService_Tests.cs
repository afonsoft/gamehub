using System;
using System.Threading.Tasks;
using Abp.UI;
using GameHub.Companies;
using GameHub.Companies.Dto;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class CompanyAppService_Tests : GameHubTestBase
    {
        private readonly ICompanyAppService _companyAppService;

        public CompanyAppService_Tests()
        {
            LoginAsHostAdmin();
            _companyAppService = Resolve<ICompanyAppService>();
        }

        [Fact]
        public async Task Dado_NenhumaEmpresa_Quando_Listar_Entao_RetornaApenasTenantDefaultEPlayerNaoSaoListados()
        {
            var output = await _companyAppService.GetAllAsync(new Abp.Application.Services.Dto.PagedAndSortedResultRequestDto());

            output.TotalCount.ShouldBe(0);
        }

        [Fact]
        public async Task Dado_DadosValidos_Quando_CriarEmpresa_Entao_RetornaTenantMapeado()
        {
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var input = new CreateOrUpdateCompanyInput
            {
                TenancyName = $"acme-games-{suffix}",
                Name = "Acme Games",
                PrimaryContactEmail = "dev@acme.local",
                Country = "BR"
            };

            var company = await _companyAppService.CreateAsync(input);

            company.ShouldNotBeNull();
            company.TenancyName.ShouldBe(input.TenancyName);
            company.Name.ShouldBe("Acme Games");
            company.PrimaryContactEmail.ShouldBe("dev@acme.local");
            company.IsActive.ShouldBeTrue();
        }

        [Fact]
        public async Task Dado_EmpresaExistente_Quando_BuscarPorTenancyName_Entao_RetornaEmpresa()
        {
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var created = await _companyAppService.CreateAsync(new CreateOrUpdateCompanyInput
            {
                TenancyName = $"studiocorp-{suffix}",
                Name = "Studio Corp",
                PrimaryContactEmail = "contact@studiocorp.local",
                Country = "US"
            });

            var company = await _companyAppService.GetByTenancyNameAsync(created.TenancyName);

            company.ShouldNotBeNull();
            company.Id.ShouldBe(created.Id);
            company.Name.ShouldBe("Studio Corp");
        }

        [Fact]
        public async Task Dado_NomeReservado_Quando_CriarEmpresa_Entao_LancaExcecao()
        {
            var input = new CreateOrUpdateCompanyInput
            {
                TenancyName = GameHubConsts.PlayerTenantName,
                Name = "Invalid",
                PrimaryContactEmail = "x@x.local"
            };

            await Should.ThrowAsync<UserFriendlyException>(() => _companyAppService.CreateAsync(input));
        }
    }
}
