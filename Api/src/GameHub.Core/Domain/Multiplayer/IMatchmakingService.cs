using System;
using System.Threading.Tasks;

namespace GameHub.Multiplayer
{
    /// <summary>
    /// Domain service for lightweight matchmaking.
    /// </summary>
    public interface IMatchmakingService
    {
        Task<MatchState> CreateMatchAsync(Guid gameId, string mode, int? maxPlayers = null);

        Task<MatchState> FindOrCreateMatchAsync(Guid gameId, string mode, int? maxPlayers = null);

        Task<MatchState> GetMatchAsync(Guid matchId);

        Task<MatchParticipant> JoinMatchAsync(Guid matchId, long? userId, string anonymousIdHash, string connectionId);

        Task<bool> LeaveMatchAsync(Guid matchId, string connectionId);

        Task UpdateMatchStateAsync(Guid matchId, string payloadJson);

        Task EndMatchAsync(Guid matchId);
    }
}
