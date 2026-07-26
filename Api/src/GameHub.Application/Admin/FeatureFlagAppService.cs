using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using GameHub.Admin.Dto;
using GameHub.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Admin
{
    /// <summary>
    /// Gerencia feature flags do painel administrativo.
    /// </summary>
    public class FeatureFlagAppService : GameHubAppServiceBase, IFeatureFlagAppService
    {
        private readonly IRepository<FeatureFlag, Guid> _featureFlagRepository;

        public FeatureFlagAppService(IRepository<FeatureFlag, Guid> featureFlagRepository)
        {
            _featureFlagRepository = featureFlagRepository;
        }

        public async Task<ListResultDto<FeatureFlagDto>> GetAllAsync()
        {
            var flags = await _featureFlagRepository.GetAll()
                .Where(f => !f.IsDeleted)
                .OrderBy(f => f.Name)
                .ToListAsync();

            return new ListResultDto<FeatureFlagDto>(ObjectMapper.Map<List<FeatureFlagDto>>(flags));
        }

        [AbpAuthorize]
        public async Task<List<string>> GetEnabledNamesAsync()
        {
            return await _featureFlagRepository.GetAll()
                .Where(f => !f.IsDeleted && f.IsEnabled && f.Name.StartsWith("sdk."))
                .Select(f => f.Name)
                .ToListAsync();
        }

        public async Task<FeatureFlagDto> ToggleAsync(Guid id, bool isEnabled)
        {
            var flag = await _featureFlagRepository.GetAsync(id);
            flag.IsEnabled = isEnabled;

            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<FeatureFlagDto>(flag);
        }
    }
}
