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

        Task<MatchState> GetMatchByRoomCodeAsync(string roomCode);

        Task<MatchParticipant> JoinMatchAsync(Guid matchId, long? userId, string anonymousIdHash, string connectionId, bool isSpectator = false);

        Task<MatchParticipant> ReactivateParticipantAsync(Guid matchId, long? userId, string anonymousIdHash, string connectionId);

        Task<bool> LeaveMatchAsync(Guid matchId, string connectionId);

        Task<bool> DisconnectAsync(string connectionId);

        Task UpdateMatchStateAsync(Guid matchId, string payloadJson, string connectionId = null);

        Task EndMatchAsync(Guid matchId);

        Task<int> CleanupExpiredMatchesAsync();

        Task<int> CleanupDisconnectedParticipantsAsync();
    }
}
