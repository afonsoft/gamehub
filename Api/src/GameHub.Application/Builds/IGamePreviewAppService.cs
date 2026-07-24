using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Builds.Dto;

namespace GameHub.Builds
{
    public interface IGamePreviewAppService : IApplicationService
    {
        Task<CreatePreviewTokenResult> CreatePreviewTokenAsync(CreatePreviewTokenInput input);

        Task<ValidatePreviewResult> ValidatePreviewAsync(ValidatePreviewInput input);
    }
}
