using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Gameplay.Dto;

namespace GameHub.Gameplay
{
    /// <summary>Manages cloud save data for players.</summary>
    public interface ICloudSaveAppService : IApplicationService
    {
        /// <summary>Returns the latest cloud save for the current player or device.</summary>
        Task<CloudSaveDto> GetAsync(GetCloudSaveInput input);

        /// <summary>Persists a cloud save, replacing any previous save for the player/device.</summary>
        Task<CloudSaveDto> SaveAsync(SaveCloudSaveInput input);
    }
}
