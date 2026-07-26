using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using GameHub.Multiplayer;
using GameHub.Multiplayer.Dto;
using GameHub.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace GameHub.Web.Hubs
{
    /// <summary>
    /// Authenticated SignalR hub for lightweight real-time multiplayer matches.
    /// </summary>
    public class GameHubMatchHub : Hub, ITransientDependency
    {
        private static readonly ConcurrentDictionary<string, Queue<DateTime>> RateWindows = new();
        private readonly IMatchmakingService _matchmakingService;
        private readonly IGameTokenProvider _gameTokenProvider;

        public GameHubMatchHub(
            IMatchmakingService matchmakingService,
            IGameTokenProvider gameTokenProvider)
        {
            _matchmakingService = matchmakingService;
            _gameTokenProvider = gameTokenProvider;
        }

        public override async Task OnConnectedAsync()
        {
            var token = Context.GetHttpContext()?.Request.Query["access_token"].ToString();
            var claims = await _gameTokenProvider.ValidateTokenAsync(token);
            if (claims == null)
            {
                claims = GetCookieClaims();
            }
            if (claims == null)
            {
                var httpContext = Context.GetHttpContext();
                if (httpContext != null)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }

                Context.Abort();
                return;
            }

            Context.Items["GameTokenClaims"] = claims;
            await base.OnConnectedAsync();
        }

        public async Task<MatchDto> CreateMatch(CreateMatchInput input)
        {
            var claims = GetClaims();
            EnsureGameScope(input.GameId, claims);
            var match = await _matchmakingService.CreateMatchAsync(input.GameId, input.Mode, input.MaxPlayers);
            await _matchmakingService.JoinMatchAsync(match.Id, claims.UserId, null, Context.ConnectionId);
            return await JoinMatchGroup(match.Id);
        }

        public async Task<MatchDto> FindOrCreateMatch(CreateMatchInput input)
        {
            var claims = GetClaims();
            EnsureGameScope(input.GameId, claims);
            var match = await _matchmakingService.FindOrCreateMatchAsync(input.GameId, input.Mode, input.MaxPlayers);
            var participant = await _matchmakingService.ReactivateParticipantAsync(
                match.Id, claims.UserId, null, Context.ConnectionId);
            if (participant == null)
            {
                await _matchmakingService.JoinMatchAsync(match.Id, claims.UserId, null, Context.ConnectionId);
            }

            return await JoinMatchGroup(match.Id);
        }

        public async Task<MatchDto> JoinMatch(JoinMatchInput input)
        {
            var claims = GetClaims();
            var match = await _matchmakingService.GetMatchAsync(input.MatchId);
            EnsureGameScope(match.GameId, claims);
            var participant = await _matchmakingService.ReactivateParticipantAsync(
                input.MatchId, claims.UserId, input.AnonymousIdHash, Context.ConnectionId);
            if (participant == null)
            {
                await _matchmakingService.JoinMatchAsync(
                    input.MatchId, claims.UserId, input.AnonymousIdHash, Context.ConnectionId, input.IsSpectator);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, input.MatchId.ToString());
            await Clients.Group(input.MatchId.ToString()).SendAsync(
                "PlayerJoined",
                new { MatchId = input.MatchId, ConnectionId = Context.ConnectionId, IsSpectator = input.IsSpectator });
            return await JoinMatchGroup(input.MatchId);
        }

        public async Task<MatchDto> JoinMatchByRoomCode(JoinMatchByRoomCodeInput input)
        {
            var claims = GetClaims();
            var match = await _matchmakingService.GetMatchByRoomCodeAsync(input.RoomCode);
            EnsureGameScope(match.GameId, claims);
            var participant = await _matchmakingService.ReactivateParticipantAsync(
                match.Id, claims.UserId, input.AnonymousIdHash, Context.ConnectionId);
            if (participant == null)
            {
                await _matchmakingService.JoinMatchAsync(
                    match.Id, claims.UserId, input.AnonymousIdHash, Context.ConnectionId, input.IsSpectator);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, match.Id.ToString());
            await Clients.Group(match.Id.ToString()).SendAsync(
                "PlayerJoined",
                new { MatchId = match.Id, ConnectionId = Context.ConnectionId, IsSpectator = input.IsSpectator });
            return await JoinMatchGroup(match.Id);
        }

        public Task<MatchDto> SpectateMatch(Guid matchId)
        {
            return JoinMatch(new JoinMatchInput { MatchId = matchId, IsSpectator = true });
        }

        public async Task LeaveMatch(Guid matchId)
        {
            await _matchmakingService.LeaveMatchAsync(matchId, Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, matchId.ToString());
            await Clients.Group(matchId.ToString()).SendAsync(
                "PlayerLeft",
                new { MatchId = matchId, ConnectionId = Context.ConnectionId });
        }

        public async Task SendMatchState(UpdateMatchStateInput input)
        {
            EnsureRateLimit($"room:{input.MatchId}");
            EnsureRateLimit($"player:{Context.UserIdentifier ?? Context.ConnectionId}");
            await _matchmakingService.UpdateMatchStateAsync(
                input.MatchId, input.PayloadJson, Context.ConnectionId);
            await Clients.Group(input.MatchId.ToString()).SendAsync(
                "MatchStateChanged",
                new { MatchId = input.MatchId, PayloadJson = input.PayloadJson });
        }

        public async Task EndMatch(Guid matchId)
        {
            var claims = GetClaims();
            var match = await _matchmakingService.GetMatchAsync(matchId);
            EnsureGameScope(match.GameId, claims);
            await _matchmakingService.EndMatchAsync(matchId);
            await Clients.Group(matchId.ToString()).SendAsync("MatchEnded", new { MatchId = matchId });
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await _matchmakingService.DisconnectAsync(Context.ConnectionId);
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
                MaxSpectators = MatchmakingService.MaxSpectatorsPerMatch,
                PayloadJson = match.PayloadJson,
                ExpiresAt = match.ExpiresAt,
                Participants = match.Participants.Select(p => new MatchParticipantDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    AnonymousIdHash = p.AnonymousIdHash,
                    ConnectionId = p.ConnectionId,
                    IsActive = p.IsActive,
                    IsSpectator = p.IsSpectator,
                    JoinedAt = p.JoinedAt
                }).ToList()
            };
        }

        private GameTokenClaims GetClaims()
        {
            if (Context.Items.TryGetValue("GameTokenClaims", out var value) && value is GameTokenClaims claims)
            {
                return claims;
            }

            throw new HubException("Unauthorized.");
        }

        private static void EnsureGameScope(Guid gameId, GameTokenClaims claims)
        {
            if (claims.GameId != Guid.Empty && claims.GameId != gameId)
            {
                throw new HubException("The token is not valid for this game.");
            }
        }

        private GameTokenClaims GetCookieClaims()
        {
            var principal = Context.User;
            var userIdValue = principal?.FindFirst("sub")?.Value
                              ?? principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (principal?.Identity?.IsAuthenticated != true || !long.TryParse(userIdValue, out var userId))
            {
                return null;
            }

            return new GameTokenClaims
            {
                UserId = userId,
                GameId = Guid.Empty,
                TenantId = int.TryParse(principal.FindFirst("tenantId")?.Value, out var tenantId) ? tenantId : null
            };
        }

        private static void EnsureRateLimit(string key)
        {
            var now = DateTime.UtcNow;
            var window = RateWindows.GetOrAdd(key, _ => new Queue<DateTime>());
            lock (window)
            {
                while (window.Count > 0 && now - window.Peek() >= TimeSpan.FromSeconds(1))
                {
                    window.Dequeue();
                }

                if (window.Count >= (key.StartsWith("room:", StringComparison.Ordinal) ? 20 : 5))
                {
                    throw new HubException("Rate limit exceeded.");
                }

                window.Enqueue(now);
            }
        }
    }
}
