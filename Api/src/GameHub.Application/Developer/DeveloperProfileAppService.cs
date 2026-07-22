using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using GameHub.Developer.Dto;
using GameHub.Developers;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Developer
{
    /// <summary>
    /// Permite ao desenvolvedor gerenciar seu próprio perfil.
    /// </summary>
    public class DeveloperProfileAppService : GameHubAppServiceBase, IDeveloperProfileAppService
    {
        private readonly IRepository<DeveloperProfile, Guid> _developerProfileRepository;

        public DeveloperProfileAppService(IRepository<DeveloperProfile, Guid> developerProfileRepository)
        {
            _developerProfileRepository = developerProfileRepository;
        }

        public async Task<DeveloperProfileDto> GetMyProfileAsync()
        {
            var userId = AbpSession.UserId ?? 0;
            var profile = await _developerProfileRepository.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return new DeveloperProfileDto();
            }

            return ObjectMapper.Map<DeveloperProfileDto>(profile);
        }

        public async Task<DeveloperProfileDto> CreateOrUpdateAsync(CreateOrUpdateDeveloperProfileInput input)
        {
            var userId = AbpSession.UserId ?? 0;
            var profile = await _developerProfileRepository.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new DeveloperProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Status = DeveloperProfileStatus.Pending
                };

                ObjectMapper.Map(input, profile);
                await _developerProfileRepository.InsertAsync(profile);
            }
            else
            {
                ObjectMapper.Map(input, profile);
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<DeveloperProfileDto>(profile);
        }
    }
}
