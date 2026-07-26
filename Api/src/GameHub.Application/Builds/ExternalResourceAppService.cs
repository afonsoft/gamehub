using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using GameHub.Authorization;
using GameHub.Builds.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Builds
{
    public class ExternalResourceAppService : GameHubAppServiceBase, IExternalResourceAppService
    {
        private readonly IRepository<ExternalResourceExemption, Guid> _exemptionRepository;

        public ExternalResourceAppService(IRepository<ExternalResourceExemption, Guid> exemptionRepository)
        {
            _exemptionRepository = exemptionRepository;
        }

        [AbpAuthorize]
        public async Task<ExternalResourceExemptionDto> RequestExemptionAsync(RequestExternalResourceExemptionInput input)
        {
            var existing = await _exemptionRepository.FirstOrDefaultAsync(e => e.GameId == input.GameId && e.Domain == input.Domain);
            if (existing != null)
            {
                existing.ProviderName = input.ProviderName ?? existing.ProviderName;
                existing.PrivacyStatementUrl = input.PrivacyStatementUrl ?? existing.PrivacyStatementUrl;
                existing.Status = ExternalResourceExemptionStatus.Pending;
                existing.RejectedAt = null;
                existing.ModeratorNotes = null;
                await CurrentUnitOfWork.SaveChangesAsync();
                return ObjectMapper.Map<ExternalResourceExemptionDto>(existing);
            }

            var exemption = new ExternalResourceExemption
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                Domain = input.Domain,
                ProviderName = input.ProviderName,
                PrivacyStatementUrl = input.PrivacyStatementUrl,
                Status = ExternalResourceExemptionStatus.Pending
            };

            await _exemptionRepository.InsertAsync(exemption);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<ExternalResourceExemptionDto>(exemption);
        }

        [AbpAuthorize]
        public async Task<List<ExternalResourceExemptionDto>> GetByGameAsync(Guid gameId)
        {
            var items = await _exemptionRepository.GetAll()
                .Where(e => e.GameId == gameId)
                .OrderByDescending(e => e.CreationTime)
                .ToListAsync();

            return ObjectMapper.Map<List<ExternalResourceExemptionDto>>(items);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_Review)]
        public async Task<ExternalResourceExemptionDto> ReviewAsync(ReviewExternalResourceExemptionInput input)
        {
            var exemption = await _exemptionRepository.GetAsync(input.Id);
            exemption.Status = input.IsApproved ? ExternalResourceExemptionStatus.Approved : ExternalResourceExemptionStatus.Rejected;
            exemption.ApprovedAt = input.IsApproved ? Clock.Now : null;
            exemption.RejectedAt = input.IsApproved ? null : Clock.Now;
            exemption.ModeratorNotes = input.ModeratorNotes;

            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<ExternalResourceExemptionDto>(exemption);
        }
    }
}
