using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Developer.Dto;

namespace GameHub.Developer
{
    public interface IDeveloperTeamAppService : IApplicationService
    {
        Task<DeveloperTeamDto> CreateTeamAsync(CreateOrUpdateDeveloperTeamInput input);

        Task<DeveloperTeamDto> UpdateTeamAsync(CreateOrUpdateDeveloperTeamInput input);

        Task<DeveloperTeamGeneralSettingsDto> UpdateGeneralSettingsAsync(UpdateTeamGeneralSettingsInput input);

        Task<DeveloperTeamDto> GetMyTeamAsync();

        Task<DeveloperTeamGeneralSettingsDto> GetGeneralSettingsAsync();

        Task<DeveloperTeamMemberDto> InviteMemberAsync(InviteMemberInput input);

        Task RemoveMemberAsync(long userId);

        Task<DeveloperTeamDto> AcceptInvitationAsync(AcceptInvitationInput input);
    }
}
