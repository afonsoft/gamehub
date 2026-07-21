using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Developer.Dto;

namespace GameHub.Developer
{
    /// <summary>
    /// Contrato de serviço de perfil de desenvolvedor.
    /// </summary>
    public interface IDeveloperProfileAppService : IApplicationService
    {
        Task<DeveloperProfileDto> GetMyProfileAsync();

        Task<DeveloperProfileDto> CreateOrUpdateAsync(CreateOrUpdateDeveloperProfileInput input);
    }
}
