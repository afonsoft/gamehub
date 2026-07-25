using System;
using System.Threading.Tasks;
using Abp.UI;
using Eaf.Middleware.Authorization.Users;
using GameHub.Authorization;
using GameHub.Authorization.Dto;
using GameHub.Developer;
using GameHub.Developer.Dto;
using GameHub.Developers;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class DeveloperTeamAppService_Tests : GameHubTestBase
    {
        private readonly IDeveloperTeamAppService _developerTeamAppService;
        private readonly IRegistrationAppService _registrationAppService;
        private readonly UserManager _userManager;

        public DeveloperTeamAppService_Tests()
        {
            _developerTeamAppService = Resolve<IDeveloperTeamAppService>();
            _registrationAppService = Resolve<IRegistrationAppService>();
            _userManager = Resolve<UserManager>();
        }

        [Fact]
        public async Task Dado_UsuarioAutenticado_Quando_CriarEquipe_Entao_DeveSerMembroDesenvolvedor()
        {
            var team = await _developerTeamAppService.CreateTeamAsync(NewTeamInput());

            team.ShouldNotBeNull();
            team.Name.ShouldBe("Team Alpha");
            team.Members.Count.ShouldBe(1);
            team.Members[0].Role.ShouldBe(DeveloperTeamRole.Developer);
        }

        [Fact]
        public async Task Dado_EquipeCriada_Quando_ObterMinhaEquipe_Entao_DeveRetornarEquipe()
        {
            var created = await _developerTeamAppService.CreateTeamAsync(NewTeamInput());

            var team = await _developerTeamAppService.GetMyTeamAsync();

            team.ShouldNotBeNull();
            team.Id.ShouldBe(created.Id);
        }

        [Fact]
        public async Task Dado_EquipeCriada_Quando_ConvidarNovoUsuario_Entao_GeraConvitePendente()
        {
            var team = await _developerTeamAppService.CreateTeamAsync(NewTeamInput());
            var invited = await RegisterUserAsync("invited", "invited@gamehub.local");

            var member = await _developerTeamAppService.InviteMemberAsync(new InviteMemberInput
            {
                Email = "invited@gamehub.local",
                Role = DeveloperTeamRole.Support
            });

            member.ShouldNotBeNull();
            member.TeamId.ShouldBe(team.Id);
            member.UserId.ShouldBe(invited.UserId);
            member.Role.ShouldBe(DeveloperTeamRole.Support);
        }

        [Fact]
        public async Task Dado_ConvitePendente_Quando_Aceitar_Entao_MembroEhAceito()
        {
            await _developerTeamAppService.CreateTeamAsync(NewTeamInput());
            var invited = await RegisterUserAsync("invited", "invited@gamehub.local");

            var member = await _developerTeamAppService.InviteMemberAsync(new InviteMemberInput
            {
                Email = "invited@gamehub.local",
                Role = DeveloperTeamRole.Developer
            });

            var originalUserId = AbpSession.UserId.Value;
            AbpSession.UserId = invited.UserId;

            try
            {
                var team = await _developerTeamAppService.AcceptInvitationAsync(new AcceptInvitationInput { Token = member.InvitationToken });

                team.ShouldNotBeNull();
                team.Members.Count.ShouldBe(2);
                team.Members.ShouldContain(m => m.UserId == invited.UserId && m.AcceptedAt.HasValue);
            }
            finally
            {
                AbpSession.UserId = originalUserId;
            }
        }

        [Fact]
        public async Task Dado_EquipeComDoisMembros_Quando_RemoverMembro_Entao_EquipeTemUmMembro()
        {
            await _developerTeamAppService.CreateTeamAsync(NewTeamInput());
            var invited = await RegisterUserAsync("invited", "invited@gamehub.local");

            var member = await _developerTeamAppService.InviteMemberAsync(new InviteMemberInput
            {
                Email = "invited@gamehub.local",
                Role = DeveloperTeamRole.Support
            });

            await _developerTeamAppService.RemoveMemberAsync(member.UserId);

            var team = await _developerTeamAppService.GetMyTeamAsync();
            team.Members.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Dado_UnicoDesenvolvedor_Quando_RemoverASiMesmo_Entao_LancaExcecao()
        {
            await _developerTeamAppService.CreateTeamAsync(NewTeamInput());
            var currentUserId = AbpSession.UserId.Value;

            await Should.ThrowAsync<UserFriendlyException>(async () =>
            {
                await _developerTeamAppService.RemoveMemberAsync(currentUserId);
            });
        }

        private static CreateOrUpdateDeveloperTeamInput NewTeamInput()
        {
            return new CreateOrUpdateDeveloperTeamInput
            {
                Name = "Team Alpha",
                PrimaryContactEmail = "team@gamehub.local",
                Country = "BR"
            };
        }

        private async Task<RegisterOutput> RegisterUserAsync(string userName, string email)
        {
            return await _registrationAppService.RegisterAsync(new RegisterInput
            {
                Name = userName,
                Surname = "Test",
                UserName = userName,
                EmailAddress = email,
                Password = "P@ssw0rd!",
                IsDeveloper = true
            });
        }
    }
}
