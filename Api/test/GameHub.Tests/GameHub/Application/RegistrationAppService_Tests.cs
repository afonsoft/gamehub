using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Eaf.Middleware.Authorization.Users;
using GameHub.Authorization;
using GameHub.Authorization.Dto;
using GameHub.Developers;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class RegistrationAppService_Tests : GameHubTestBase
    {
        private readonly IRegistrationAppService _registrationAppService;
        private readonly IRepository<DeveloperProfile, Guid> _developerProfileRepository;
        private readonly UserManager _userManager;

        public RegistrationAppService_Tests()
        {
            _registrationAppService = Resolve<IRegistrationAppService>();
            _developerProfileRepository = Resolve<IRepository<DeveloperProfile, Guid>>();
            _userManager = Resolve<UserManager>();
        }

        [Fact]
        public async Task Dado_DadosValidos_Quando_RegistrarPlayer_Entao_CriaUsuarioEAtribuiRolePlayer()
        {
            var input = new RegisterInput
            {
                Name = "Player",
                Surname = "One",
                UserName = "playerone",
                EmailAddress = "playerone@gamehub.local",
                Password = "P@ssw0rd!",
                IsDeveloper = false
            };

            var result = await _registrationAppService.RegisterAsync(input);

            result.ShouldNotBeNull();
            result.UserName.ShouldBe("playerone");

            var user = await GetUserByUserNameAsync("playerone");
            user.ShouldNotBeNull();
            (await _userManager.IsInRoleAsync(user, "Player")).ShouldBeTrue();
            (await _userManager.IsInRoleAsync(user, "Developer")).ShouldBeFalse();
        }

        [Fact]
        public async Task Dado_DadosValidos_Quando_RegistrarDeveloper_Entao_CriaUsuarioPerfilDeveloperEAtribuiRoles()
        {
            var input = new RegisterInput
            {
                Name = "Dev",
                Surname = "Two",
                UserName = "devtwo",
                EmailAddress = "devtwo@gamehub.local",
                Password = "P@ssw0rd!",
                IsDeveloper = true
            };

            var result = await _registrationAppService.RegisterAsync(input);

            result.ShouldNotBeNull();
            result.UserName.ShouldBe("devtwo");

            var user = await GetUserByUserNameAsync("devtwo");
            user.ShouldNotBeNull();
            (await _userManager.IsInRoleAsync(user, "Player")).ShouldBeTrue();
            (await _userManager.IsInRoleAsync(user, "Developer")).ShouldBeTrue();

            var profile = await UsingDbContextAsync(async context =>
                await context.DeveloperProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id));
            profile.ShouldNotBeNull();
            profile.Status.ShouldBe(DeveloperProfileStatus.Active);
        }

        [Fact]
        public async Task Dado_UserNameDuplicado_Quando_Registrar_Entao_LancaExcecaoAmigavel()
        {
            var input = new RegisterInput
            {
                Name = "Existing",
                Surname = "User",
                UserName = "admin",
                EmailAddress = "unique@gamehub.local",
                Password = "P@ssw0rd!",
                IsDeveloper = false
            };

            await Should.ThrowAsync<Abp.UI.UserFriendlyException>(async () => await _registrationAppService.RegisterAsync(input));
        }
    }
}
