using System;
using System.Threading.Tasks;
using Abp.UI;
using GameHub.Companies;
using GameHub.Companies.Dto;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class CompanyEmployeeAppService_Tests : GameHubTestBase
    {
        private readonly ICompanyAppService _companyAppService;
        private readonly ICompanyEmployeeAppService _companyEmployeeAppService;

        public CompanyEmployeeAppService_Tests()
        {
            LoginAsHostAdmin();
            _companyAppService = Resolve<ICompanyAppService>();
            _companyEmployeeAppService = Resolve<ICompanyEmployeeAppService>();
        }

        [Fact]
        public async Task Dado_EmpresaCriada_Quando_RegistrarEAdicionarFuncionario_Entao_FuncionarioApareceNaLista()
        {
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var company = await _companyAppService.CreateAsync(new CreateOrUpdateCompanyInput
            {
                TenancyName = $"gamescorp-{suffix}",
                Name = "Games Corp",
                PrimaryContactEmail = "contact@gamescorp.local",
                Country = "BR"
            });

            var employee = await _companyEmployeeAppService.RegisterAndJoinAsync(new JoinCompanyInput
            {
                TenancyName = company.TenancyName,
                UserName = $"newdev-{suffix}",
                Name = "New",
                Surname = "Dev",
                EmailAddress = $"newdev-{suffix}@gamescorp.local",
                Password = "P@ssw0rd!",
                Role = "Developer"
            });

            employee.ShouldNotBeNull();
            employee.UserName.ShouldBe($"newdev-{suffix}");
            employee.Role.ShouldBe("Developer");

            var employees = await _companyEmployeeAppService.GetEmployeesAsync(company.Id);
            employees.Count.ShouldBe(1);
            employees[0].UserName.ShouldBe($"newdev-{suffix}");
        }

        [Fact]
        public async Task Dado_DoisFuncionarios_Quando_DefinirDefault_Entao_SomenteUmEhDefault()
        {
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var company = await _companyAppService.CreateAsync(new CreateOrUpdateCompanyInput
            {
                TenancyName = $"teamtwo-{suffix}",
                Name = "Team Two",
                PrimaryContactEmail = "contact@teamtwo.local",
                Country = "US"
            });

            var first = await _companyEmployeeAppService.RegisterAndJoinAsync(new JoinCompanyInput
            {
                TenancyName = company.TenancyName,
                UserName = $"firstdev-{suffix}",
                Name = "First",
                Surname = "Dev",
                EmailAddress = $"first-{suffix}@teamtwo.local",
                Password = "P@ssw0rd!",
                Role = "Developer"
            });

            var second = await _companyEmployeeAppService.RegisterAndJoinAsync(new JoinCompanyInput
            {
                TenancyName = company.TenancyName,
                UserName = $"seconddev-{suffix}",
                Name = "Second",
                Surname = "Dev",
                EmailAddress = $"second-{suffix}@teamtwo.local",
                Password = "P@ssw0rd!",
                Role = "Support"
            });

            await _companyEmployeeAppService.SetDefaultAsync(new SetDefaultEmployeeInput
            {
                TenantId = company.Id,
                UserId = second.UserId
            });

            var employees = await _companyEmployeeAppService.GetEmployeesAsync(company.Id);
            employees.ShouldContain(e => e.UserId == first.UserId && !e.IsDefault);
            employees.ShouldContain(e => e.UserId == second.UserId && e.IsDefault);
        }

        [Fact]
        public async Task Dado_RegistroDuplicado_Quando_JoinCompany_Entao_LancaExcecao()
        {
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var company = await _companyAppService.CreateAsync(new CreateOrUpdateCompanyInput
            {
                TenancyName = $"duptest-{suffix}",
                Name = "Dup Test",
                PrimaryContactEmail = "contact@duptest.local",
                Country = "BR"
            });

            await _companyEmployeeAppService.RegisterAndJoinAsync(new JoinCompanyInput
            {
                TenancyName = company.TenancyName,
                UserName = $"dupuser-{suffix}",
                Name = "Dup",
                Surname = "User",
                EmailAddress = $"dup-{suffix}@duptest.local",
                Password = "P@ssw0rd!",
                Role = "Developer"
            });

            await Should.ThrowAsync<UserFriendlyException>(() => _companyEmployeeAppService.RegisterAndJoinAsync(new JoinCompanyInput
            {
                TenancyName = company.TenancyName,
                UserName = $"dupuser-{suffix}",
                Name = "Dup",
                Surname = "User",
                EmailAddress = $"other-{suffix}@duptest.local",
                Password = "P@ssw0rd!",
                Role = "Developer"
            }));
        }
    }
}
