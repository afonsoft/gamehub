using System;
using System.Threading.Tasks;
using Abp.Dependency;
using GameHub.Multiplayer;
using GameHub.Security;
using Microsoft.AspNetCore.SignalR;
using GameHub.Web.Multiplayer;

namespace GameHub.Web.Hubs
{
    /// <summary>
    /// WebRTC signaling hub with SignalR relay fallback.
    /// </summary>
    public class NetworkSignalRHub : Hub, ITransientDependency
    {
        private readonly IMatchmakingService _matchmakingService;
        private readonly IGameTokenProvider _gameTokenProvider;
        private readonly NetworkPeerRegistry _peerRegistry;

        public NetworkSignalRHub(
            IMatchmakingService matchmakingService,
            IGameTokenProvider gameTokenProvider,
            NetworkPeerRegistry peerRegistry)
        {
            _matchmakingService = matchmakingService;
            _gameTokenProvider = gameTokenProvider;
            _peerRegistry = peerRegistry;
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
                Context.Abort();
                return;
            }

            Context.Items["GameTokenClaims"] = claims;
            await base.OnConnectedAsync();
        }

        public async Task<object> JoinLobby(Guid gameId, string mode, int? maxPlayers)
        {
            var claims = GetClaims();
            if (claims.GameId != Guid.Empty && claims.GameId != gameId)
            {
                throw new HubException("The token is not valid for this game.");
            }

            var match = await _matchmakingService.FindOrCreateMatchAsync(gameId, mode ?? "default", maxPlayers);
            var participant = await _matchmakingService.ReactivateParticipantAsync(
                match.Id, claims.UserId, null, Context.ConnectionId);
            if (participant == null)
            {
                await _matchmakingService.JoinMatchAsync(match.Id, claims.UserId, null, Context.ConnectionId);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"network:{match.Id:N}");
            await _peerRegistry.RegisterAsync(
                Context.ConnectionId,
                claims,
                gameId,
                match.Id);
            return new { MatchId = match.Id, PeerId = Context.ConnectionId, RoomCode = match.RoomCode };
        }

        public async Task Signal(string peerId, object payload)
        {
            var claims = GetClaims();
            var presence = await _peerRegistry.GetAsync(claims.TenantId, Context.ConnectionId);
            if (presence == null)
            {
                throw new HubException("Join a lobby before sending signals.");
            }

            await Clients.Client(peerId).SendAsync("Signal", Context.ConnectionId, payload);
        }

        public async Task Heartbeat()
        {
            var claims = GetClaims();
            var presence = await _peerRegistry.GetAsync(claims.TenantId, Context.ConnectionId);
            if (presence == null)
            {
                throw new HubException("Join a lobby before sending heartbeats.");
            }

            await _peerRegistry.RefreshAsync(
                claims.TenantId,
                Context.ConnectionId,
                presence.MatchId);
        }

        public async Task Broadcast(string channel, object payload)
        {
            var claims = GetClaims();
            var presence = await _peerRegistry.GetAsync(claims.TenantId, Context.ConnectionId);
            if (presence == null)
            {
                throw new HubException("Join a lobby before broadcasting.");
            }

            var normalizedChannel = channel is "reliable" or "unreliable" ? channel : "reliable";
            await Clients.Group($"network:{presence.MatchId:N}").SendAsync(
                "Broadcast",
                normalizedChannel,
                Context.ConnectionId,
                payload);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var claims = Context.Items.TryGetValue("GameTokenClaims", out var value)
                ? value as GameTokenClaims
                : null;
            await _peerRegistry.RemoveAsync(claims?.TenantId, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        private GameTokenClaims GetClaims()
        {
            return Context.Items.TryGetValue("GameTokenClaims", out var value)
                   && value is GameTokenClaims claims
                ? claims
                : throw new HubException("Unauthorized.");
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
                GameId = Guid.Empty
            };
        }
    }

    /// <summary>
    /// Tracks SignalR peers and the lobby they joined.
    /// </summary>
    public class NetworkPeerRegistry
    {
        private readonly IMultiplayerPresenceStore _presenceStore;
        private readonly MultiplayerPresenceOptions _options;

        public NetworkPeerRegistry(
            IMultiplayerPresenceStore presenceStore,
            Microsoft.Extensions.Options.IOptions<MultiplayerPresenceOptions> options)
        {
            _presenceStore = presenceStore;
            _options = options.Value;
        }

        public Task RegisterAsync(
            string connectionId,
            GameTokenClaims claims,
            Guid gameId,
            Guid matchId)
        {
            var now = DateTimeOffset.UtcNow;
            return _presenceStore.RegisterAsync(
                new MultiplayerPresenceEntry
                {
                    TenantId = claims.TenantId,
                    GameId = gameId,
                    MatchId = matchId,
                    ConnectionId = connectionId,
                    UserId = claims.UserId,
                    InstanceId = _options.InstanceId,
                    JoinedAt = now,
                    LastSeenAt = now
                },
                _options.EntryTtl);
        }

        public Task<MultiplayerPresenceEntry> GetAsync(int? tenantId, string connectionId)
        {
            return _presenceStore.GetByConnectionAsync(tenantId, connectionId);
        }

        public Task RefreshAsync(int? tenantId, string connectionId, Guid matchId)
        {
            return _presenceStore.RefreshAsync(
                tenantId,
                matchId,
                connectionId,
                _options.EntryTtl);
        }

        public Task RemoveAsync(int? tenantId, string connectionId)
        {
            return _presenceStore.RemoveByConnectionAsync(tenantId, connectionId);
        }
    }
}
