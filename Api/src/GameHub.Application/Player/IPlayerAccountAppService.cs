using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Player.Dto;

namespace GameHub.Player
{
    /// <summary>
    /// Manages player favorites and recent games.
    /// </summary>
    public interface IPlayerAccountAppService : IApplicationService
    {
        Task<List<PlayerFavoriteDto>> GetFavoritesAsync();

        Task<bool> ToggleFavoriteAsync(ToggleFavoriteInput input);

        Task<List<PlayerRecentGameDto>> GetRecentAsync(GetRecentInput input);

        Task TrackPlayAsync(TrackPlayInput input);

        Task MergeLocalDataAsync(MergePlayerDataInput input);
    }
}
