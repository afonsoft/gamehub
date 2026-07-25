using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Developer.Dto;

namespace GameHub.Developer
{
    public interface IDeveloperBillingProfileAppService : IApplicationService
    {
        Task<DeveloperBillingProfileDto> GetByTeamAsync(Guid teamId);

        Task<DeveloperBillingProfileDto> SaveAsync(SaveDeveloperBillingProfileInput input);
    }
}
