using System.Threading.Tasks;
using Abp.Dependency;
using GameHub.Multiplayer;

namespace GameHub.Jobs
{
    /// <summary>
    /// Closes expired rooms and removes participants whose reconnect grace period elapsed.
    /// </summary>
    public class CleanupMultiplayerJob : ITransientDependency
    {
        private readonly IMatchmakingService _matchmakingService;

        public CleanupMultiplayerJob(IMatchmakingService matchmakingService)
        {
            _matchmakingService = matchmakingService;
        }

        public async Task Execute()
        {
            await _matchmakingService.CleanupDisconnectedParticipantsAsync();
            await _matchmakingService.CleanupExpiredMatchesAsync();
        }
    }
}
