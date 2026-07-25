using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using GameHub.Authorization;
using GameHub.Developer.Dto;
using GameHub.Developers;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Developer
{
    public class DeveloperBillingProfileAppService : GameHubAppServiceBase, IDeveloperBillingProfileAppService
    {
        private readonly IRepository<DeveloperBillingProfile, Guid> _billingProfileRepository;
        private readonly IRepository<DeveloperTeamMember, Guid> _teamMemberRepository;

        public DeveloperBillingProfileAppService(
            IRepository<DeveloperBillingProfile, Guid> billingProfileRepository,
            IRepository<DeveloperTeamMember, Guid> teamMemberRepository)
        {
            _billingProfileRepository = billingProfileRepository;
            _teamMemberRepository = teamMemberRepository;
        }

        [AbpAuthorize(GameHubPermissions.Pages_Developer_Profile)]
        public async Task<DeveloperBillingProfileDto> GetByTeamAsync(Guid teamId)
        {
            await EnsureTeamAccessAsync(teamId);

            var profile = await _billingProfileRepository.FirstOrDefaultAsync(b => b.TeamId == teamId);
            if (profile == null)
            {
                return new DeveloperBillingProfileDto { TeamId = teamId };
            }

            return ObjectMapper.Map<DeveloperBillingProfileDto>(profile);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Developer_Profile)]
        public async Task<DeveloperBillingProfileDto> SaveAsync(SaveDeveloperBillingProfileInput input)
        {
            await EnsureTeamAccessAsync(input.TeamId, requireBillingOrDeveloper: true);

            var profile = await _billingProfileRepository.FirstOrDefaultAsync(b => b.TeamId == input.TeamId);
            if (profile == null)
            {
                profile = new DeveloperBillingProfile
                {
                    Id = Guid.NewGuid(),
                    TenantId = AbpSession.TenantId,
                    TeamId = input.TeamId,
                    IsApproved = false,
                    IsPendingReview = true
                };
                await _billingProfileRepository.InsertAsync(profile);
            }
            else
            {
                profile.IsApproved = false;
                profile.IsPendingReview = true;
            }

            profile.TaxId = input.TaxId;
            profile.Address = input.Address;
            profile.PaymentMethodPlaceholder = input.PaymentMethodPlaceholder;

            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<DeveloperBillingProfileDto>(profile);
        }

        private async Task EnsureTeamAccessAsync(Guid teamId, bool requireBillingOrDeveloper = false)
        {
            var member = await _teamMemberRepository.FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == AbpSession.UserId.Value);
            if (member == null)
            {
                throw new AbpAuthorizationException("You are not a member of this team.");
            }

            if (requireBillingOrDeveloper && member.Role != DeveloperTeamRole.Developer && member.Role != DeveloperTeamRole.Billing)
            {
                throw new AbpAuthorizationException("Only team developers or billing members can manage billing information.");
            }
        }
    }
}
