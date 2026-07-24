using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Developer.Dto;

namespace GameHub.Developer
{
    /// <summary>
    /// Exposes estimated earnings for the current developer.
    /// </summary>
    public interface IDeveloperEarningsAppService : IApplicationService
    {
        Task<DeveloperEarningsDto> GetEarningsAsync(GetDeveloperEarningsInput input);
    }
}
