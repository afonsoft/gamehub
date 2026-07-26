using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Runtime.Session;
using GameHub.Multiplayer;
using GameHub.Multiplayer.Dto;
using Microsoft.AspNetCore.SignalR;

namespace GameHub.Web.Hubs
{
    /// <summary>
    /// SignalR hub for lightweight real-time multiplayer matches.
    /// </summary>
    public class GameHubMatchHub : Hub, ITransientDependency
    {
        private readonly IMatchmakingService _matchmakingService;

        public IAbpSession AbpSession { get; set; }

        public GameHubMatchHub(IMatchmakingService matchmakingService)
        {
            _matchmakingService = matchmakingService;
            AbpSession = NullAbpSession.Instance;
        }

        public async Task<MatchDto> CreateMatch(CreateMatchInput input)
        {
            var match = await _matchmakingService.CreateMatchAsync(input.GameId, input.Mode, input.MaxPlayers);
            return await JoinMatchGroup(match.Id);
        }

        public async Task<MatchDto> FindOrCreateMatch(CreateMatchInput input)
        {
            var match = await _matchmakingService.FindOrCreateMatchAsync(input.GameId, input.Mode, input.MaxPlayers);
            return await JoinMatchGroup(match.Id);
        }

        public async Task<MatchDto> JoinMatch(JoinMatchInput input)
        {
            await _matchmakingService.JoinMatchAsync(input.MatchId, AbpSession.UserId, input.AnonymousIdHash, Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, input.MatchId.ToString());
            await Clients.Group(input.MatchId.ToString()).SendAsync("PlayerJoined", new { MatchId = input.MatchId, ConnectionId = Context.ConnectionId });
            return await JoinMatchGroup(input.MatchId);
        }

        public async Task<MatchDto> JoinMatchByRoomCode(JoinMatchByRoomCodeInput input)
        {
            // Implemented through the application service to keep hub thin.
            throw new NotImplementedException("Use MultiplayerAppService.JoinMatchByRoomCodeAsync via HTTP or implement a lookup here.");
        }

        public async Task LeaveMatch(Guid matchId)
        {
            await _matchmakingService.LeaveMatchAsync(matchId, Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, matchId.ToString());
            await Clients.Group(matchId.ToString()).SendAsync("PlayerLeft", new { MatchId = matchId, ConnectionId = Context.ConnectionId });
        }

        public async Task SendMatchState(UpdateMatchStateInput input)
        {
            await _matchmakingService.UpdateMatchStateAsync(input.MatchId, input.PayloadJson);
            await Clients.Group(input.MatchId.ToString()).SendAsync("MatchStateChanged", new { MatchId = input.MatchId, PayloadJson = input.PayloadJson });
        }

        public async Task EndMatch(Guid matchId)
        {
            await _matchmakingService.EndMatchAsync(matchId);
            await Clients.Group(matchId.ToString()).SendAsync("MatchEnded", new { MatchId = matchId });
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Optional: leave any active matches by connection id.
            await base.OnDisconnectedAsync(exception);
        }

        private async Task<MatchDto> JoinMatchGroup(Guid matchId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, matchId.ToString());
            var match = await _matchmakingService.GetMatchAsync(matchId);
            return new MatchDto
            {
                Id = match.Id,
                GameId = match.GameId,
                RoomCode = match.RoomCode,
                Mode = match.Mode,
                Status = match.Status.ToString(),
                MaxPlayers = match.MaxPlayers,
                PayloadJson = match.PayloadJson,
                ExpiresAt = match.ExpiresAt
            };
        }
    }
}
