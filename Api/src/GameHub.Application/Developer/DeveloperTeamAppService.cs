using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using Abp.UI;
using GameHub.Authorization;
using GameHub.Developer.Dto;
using GameHub.Developers;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Developer
{
    public class DeveloperTeamAppService : GameHubAppServiceBase, IDeveloperTeamAppService
    {
        private readonly IRepository<DeveloperTeam, Guid> _teamRepository;
        private readonly IRepository<DeveloperTeamMember, Guid> _teamMemberRepository;

        public DeveloperTeamAppService(
            IRepository<DeveloperTeam, Guid> teamRepository,
            IRepository<DeveloperTeamMember, Guid> teamMemberRepository)
        {
            _teamRepository = teamRepository;
            _teamMemberRepository = teamMemberRepository;
        }

        [AbpAuthorize(GameHubPermissions.Pages_Developer_Profile)]
        public async Task<DeveloperTeamDto> CreateTeamAsync(CreateOrUpdateDeveloperTeamInput input)
        {
            var userId = AbpSession.UserId.Value;
            var existingMember = await _teamMemberRepository.FirstOrDefaultAsync(m => m.UserId == userId);
            if (existingMember != null)
            {
                throw new UserFriendlyException("User already belongs to a team.");
            }

            var team = new DeveloperTeam
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                Name = input.Name,
                PrimaryContactEmail = input.PrimaryContactEmail,
                Country = input.Country,
                CreatedAt = Clock.Now
            };

            await _teamRepository.InsertAsync(team);

            var member = new DeveloperTeamMember
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                TeamId = team.Id,
                UserId = userId,
                Role = DeveloperTeamRole.Developer,
                InvitedAt = Clock.Now,
                AcceptedAt = Clock.Now
            };

            await _teamMemberRepository.InsertAsync(member);
            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetTeamDtoAsync(team.Id);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Developer_Profile)]
        public async Task<DeveloperTeamDto> UpdateTeamAsync(CreateOrUpdateDeveloperTeamInput input)
        {
            var team = await GetCurrentUserTeamAsync(requireDeveloper: true);

            team.Name = input.Name;
            team.PrimaryContactEmail = input.PrimaryContactEmail;
            team.Country = input.Country;

            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetTeamDtoAsync(team.Id);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Developer_Profile)]
        public async Task<DeveloperTeamDto> GetMyTeamAsync()
        {
            var team = await GetCurrentUserTeamAsync();
            if (team == null)
            {
                return null;
            }

            return await GetTeamDtoAsync(team.Id);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Developer_Profile)]
        public async Task<DeveloperTeamMemberDto> InviteMemberAsync(InviteMemberInput input)
        {
            var team = await GetCurrentUserTeamAsync(requireDeveloper: true);

            var invitedUser = await UserManager.FindByEmailAsync(input.Email);
            if (invitedUser == null)
            {
                throw new UserFriendlyException("User not found for the given email.");
            }

            var existingMember = await _teamMemberRepository.FirstOrDefaultAsync(m => m.UserId == invitedUser.Id);
            if (existingMember != null)
            {
                throw new UserFriendlyException("User already belongs to a team.");
            }

            var token = Guid.NewGuid().ToString("N");
            var member = new DeveloperTeamMember
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                TeamId = team.Id,
                UserId = invitedUser.Id,
                Role = input.Role,
                InvitedAt = Clock.Now,
                InvitationToken = token
            };

            await _teamMemberRepository.InsertAsync(member);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<DeveloperTeamMemberDto>(member);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Developer_Profile)]
        public async Task RemoveMemberAsync(long userId)
        {
            var team = await GetCurrentUserTeamAsync(requireDeveloper: true);
            var member = await _teamMemberRepository.FirstOrDefaultAsync(m => m.TeamId == team.Id && m.UserId == userId);

            if (member == null)
            {
                throw new UserFriendlyException("Member not found.");
            }

            if (member.UserId == AbpSession.UserId.Value && member.Role == DeveloperTeamRole.Developer)
            {
                var otherDevelopers = await _teamMemberRepository.CountAsync(m => m.TeamId == team.Id && m.UserId != userId && m.Role == DeveloperTeamRole.Developer);
                if (otherDevelopers == 0)
                {
                    throw new UserFriendlyException("Cannot remove the only developer of the team.");
                }
            }

            await _teamMemberRepository.DeleteAsync(member);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [AbpAuthorize]
        public async Task<DeveloperTeamDto> AcceptInvitationAsync(AcceptInvitationInput input)
        {
            var member = await _teamMemberRepository.FirstOrDefaultAsync(m => m.InvitationToken == input.Token);
            if (member == null)
            {
                throw new UserFriendlyException("Invalid invitation token.");
            }

            if (member.UserId != AbpSession.UserId.Value)
            {
                throw new AbpAuthorizationException("This invitation is not for the current user.");
            }

            member.AcceptedAt = Clock.Now;
            member.InvitationToken = null;
            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetTeamDtoAsync(member.TeamId);
        }

        private async Task<DeveloperTeam> GetCurrentUserTeamAsync(bool requireDeveloper = false)
        {
            var userId = AbpSession.UserId.Value;
            var member = await _teamMemberRepository.FirstOrDefaultAsync(m => m.UserId == userId);
            if (member == null)
            {
                if (requireDeveloper)
                {
                    throw new UserFriendlyException("You are not a member of a developer team.");
                }

                return null;
            }

            if (requireDeveloper && member.Role != DeveloperTeamRole.Developer && member.Role != DeveloperTeamRole.Billing)
            {
                throw new AbpAuthorizationException("Only team developers or billing members can perform this action.");
            }

            return await _teamRepository.GetAsync(member.TeamId);
        }

        private async Task<DeveloperTeamDto> GetTeamDtoAsync(Guid teamId)
        {
            var team = await _teamRepository.GetAll()
                .Where(t => t.Id == teamId)
                .Include(t => t.Members)
                .FirstOrDefaultAsync();

            if (team == null)
            {
                return null;
            }

            var dto = ObjectMapper.Map<DeveloperTeamDto>(team);

            foreach (var memberDto in dto.Members)
            {
                var user = await UserManager.FindByIdAsync(memberDto.UserId.ToString());
                if (user != null)
                {
                    memberDto.UserName = user.UserName;
                    memberDto.Email = user.EmailAddress;
                }
            }

            return dto;
        }
    }
}
