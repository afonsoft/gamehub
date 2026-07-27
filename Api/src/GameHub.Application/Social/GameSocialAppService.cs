using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Abp.Timing;
using GameHub.Catalog;
using GameHub.Exceptions;
using GameHub.Moderation;
using GameHub.Multiplayer;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Social
{
    /// <summary>
    /// Implements authenticated social actions without exposing internal user data.
    /// </summary>
    public class GameSocialAppService : GameHubAppServiceBase, IGameSocialAppService
    {
        private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan IdempotencyWindow = TimeSpan.FromMinutes(5);
        private const int MaxReportsPerMinute = 10;

        private readonly IRepository<GameInvite, Guid> _inviteRepository;
        private readonly IRepository<GameNotification, Guid> _notificationRepository;
        private readonly IRepository<MatchState, Guid> _matchRepository;
        private readonly IRepository<MatchParticipant, Guid> _participantRepository;
        private readonly IRepository<UserReport, Guid> _reportRepository;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IMultiplayerPresenceStore _presenceStore;
        private readonly ITypedCache<string, string> _rateLimitCache;
        private readonly ITypedCache<string, string> _idempotencyCache;

        public GameSocialAppService(
            IRepository<GameInvite, Guid> inviteRepository,
            IRepository<GameNotification, Guid> notificationRepository,
            IRepository<MatchState, Guid> matchRepository,
            IRepository<MatchParticipant, Guid> participantRepository,
            IRepository<UserReport, Guid> reportRepository,
            IRepository<Game, Guid> gameRepository,
            IMultiplayerPresenceStore presenceStore,
            ICacheManager cacheManager)
        {
            _inviteRepository = inviteRepository;
            _notificationRepository = notificationRepository;
            _matchRepository = matchRepository;
            _participantRepository = participantRepository;
            _reportRepository = reportRepository;
            _gameRepository = gameRepository;
            _presenceStore = presenceStore;
            _rateLimitCache = cacheManager
                .GetCache("GameHub.Social.ReportRateLimit")
                .AsTyped<string, string>();
            _idempotencyCache = cacheManager
                .GetCache("GameHub.Social.ReportIdempotency")
                .AsTyped<string, string>();
        }

        public async Task<GameInviteDto> InvitePlayerAsync(InvitePlayerInput input)
        {
            var inviterUserId = RequireUserId();
            var match = await _matchRepository.GetAll()
                .FirstOrDefaultAsync(item => item.Id == input.MatchId && item.GameId == input.GameId);

            if (match == null || match.Status == MatchStatus.Ended || match.Status == MatchStatus.Cancelled)
            {
                throw new GameHubException(
                    GameHubErrorCodes.InvalidContext,
                    "A partida não está disponível.",
                    retryable: false);
            }

            await EnsureActiveParticipantAsync(input.MatchId, inviterUserId);
            await UserManager.GetUserByIdAsync(input.InviteeUserId);

            var expiresAt = input.ExpiresAt ?? Clock.Now.AddMinutes(15);
            if (expiresAt <= Clock.Now)
            {
                throw new GameHubException(
                    GameHubErrorCodes.ValidationFailed,
                    "O convite deve expirar no futuro.",
                    retryable: false);
            }

            var existing = await _inviteRepository.FirstOrDefaultAsync(item =>
                item.MatchId == input.MatchId
                && item.InviterUserId == inviterUserId
                && item.InviteeUserId == input.InviteeUserId
                && item.Status == "pending"
                && item.ExpiresAt > Clock.Now);
            if (existing != null)
            {
                return MapInvite(existing);
            }

            var invite = new GameInvite
            {
                Id = SequentialGuidGenerator.Instance.Create(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                MatchId = input.MatchId,
                InviterUserId = inviterUserId,
                InviteeUserId = input.InviteeUserId,
                Status = "pending",
                ExpiresAt = expiresAt,
                CreationTime = Clock.Now
            };
            await _inviteRepository.InsertAsync(invite);

            await _notificationRepository.InsertAsync(new GameNotification
            {
                Id = SequentialGuidGenerator.Instance.Create(),
                TenantId = AbpSession.TenantId,
                UserId = input.InviteeUserId,
                NotificationType = "match_invite",
                Title = "You received a match invite.",
                PayloadJson = JsonSerializer.Serialize(new
                {
                    inviteId = invite.Id,
                    gameId = invite.GameId,
                    matchId = invite.MatchId
                }),
                CreationTime = Clock.Now
            });
            await CurrentUnitOfWork.SaveChangesAsync();
            return MapInvite(invite);
        }

        public async Task<GameInviteDto> AcceptInviteAsync(Guid inviteId)
        {
            var userId = RequireUserId();
            var invite = await _inviteRepository.GetAll()
                .FirstOrDefaultAsync(item => item.Id == inviteId && item.InviteeUserId == userId);
            if (invite == null || invite.Status != "pending" || invite.ExpiresAt <= Clock.Now)
            {
                throw new GameHubException(
                    GameHubErrorCodes.InvalidContext,
                    "O convite não está disponível.",
                    retryable: false);
            }

            await EnsureActiveParticipantAsync(invite.MatchId, userId);
            invite.Status = "accepted";
            invite.AcceptedAt = Clock.Now;
            var notification = await _notificationRepository.FirstOrDefaultAsync(item =>
                item.UserId == userId
                && item.NotificationType == "match_invite"
                && item.PayloadJson.Contains(invite.Id.ToString()));
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = Clock.Now;
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            return MapInvite(invite);
        }

        public async Task<GameNotificationListDto> GetNotificationsAsync()
        {
            var userId = RequireUserId();
            var notifications = await _notificationRepository.GetAll()
                .Where(item => item.UserId == userId)
                .OrderByDescending(item => item.CreationTime)
                .Take(100)
                .ToListAsync();
            return new GameNotificationListDto(notifications.Select(MapNotification).ToList());
        }

        public async Task MarkNotificationReadAsync(Guid notificationId)
        {
            var userId = RequireUserId();
            var notification = await _notificationRepository.FirstOrDefaultAsync(
                item => item.Id == notificationId && item.UserId == userId);
            if (notification == null)
            {
                return;
            }

            notification.IsRead = true;
            notification.ReadAt = Clock.Now;
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        public async Task<PresenceDto> GetPresenceAsync(long userId)
        {
            RequireUserId();
            var entry = await _presenceStore.GetByUserAsync(AbpSession.TenantId, userId);
            return new PresenceDto
            {
                UserId = userId,
                State = entry == null ? "offline" : "online"
            };
        }

        public async Task ReportPlayerAsync(ReportPlayerInput input)
        {
            var userId = RequireUserId();

            if (input.ReportedUserId <= 0 || input.ReportedUserId == userId)
            {
                throw new GameHubException(
                    GameHubErrorCodes.ValidationFailed,
                    "É necessário reportar um jogador diferente de você.",
                    retryable: false);
            }

            if (string.IsNullOrWhiteSpace(input.Reason) || input.Reason.Trim().Length > 128)
            {
                throw new GameHubException(
                    GameHubErrorCodes.ValidationFailed,
                    "O motivo do report é obrigatório e deve ter até 128 caracteres.",
                    retryable: false);
            }

            if (!await _gameRepository.GetAll().AnyAsync(g => g.Id == input.GameId && !g.IsDeleted))
            {
                throw new GameHubException(
                    GameHubErrorCodes.InvalidContext,
                    "O jogo informado não foi encontrado.",
                    retryable: false);
            }

            await EnsureRateLimitAsync(input.GameId);

            var idempotencyKey = BuildIdempotencyKey(input.GameId, input.ClientRequestId);
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var existingId = await _idempotencyCache.GetOrDefaultAsync(idempotencyKey);
                if (existingId != null)
                {
                    return;
                }
            }

            await _reportRepository.InsertAsync(new UserReport
            {
                Id = SequentialGuidGenerator.Instance.Create(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                UserId = input.ReportedUserId,
                Reason = input.Reason.Trim(),
                Status = UserReportStatus.Open,
                CreationTime = Clock.Now
            });
            await CurrentUnitOfWork.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                await _idempotencyCache.SetAsync(
                    idempotencyKey,
                    "1",
                    absoluteExpireTime: DateTimeOffset.UtcNow.Add(IdempotencyWindow));
            }
        }

        private async Task EnsureRateLimitAsync(Guid gameId)
        {
            var key =
                $"gamehub:social:report:" +
                $"{AbpSession.TenantId?.ToString() ?? "host"}:" +
                $"{AbpSession.UserId}:" +
                $"{gameId:N}";

            var current = await _rateLimitCache.GetOrDefaultAsync(key);
            var count = int.TryParse(current, out var parsed) ? parsed : 0;

            if (count >= MaxReportsPerMinute)
            {
                throw new GameHubException(
                    GameHubErrorCodes.RateLimited,
                    "Limite de envio de reports excedido. Tente novamente mais tarde.",
                    retryable: true);
            }

            await _rateLimitCache.SetAsync(
                key,
                (count + 1).ToString(),
                absoluteExpireTime: DateTimeOffset.UtcNow.Add(RateLimitWindow));
        }

        private static string BuildIdempotencyKey(Guid gameId, string clientRequestId)
        {
            if (string.IsNullOrWhiteSpace(clientRequestId))
                return null;

            return $"gamehub:social:report:idempotency:{gameId:N}:{clientRequestId.Trim()}";
        }

        private async Task EnsureActiveParticipantAsync(Guid matchId, long userId)
        {
            var isParticipant = await _participantRepository.GetAll()
                .AnyAsync(item => item.MatchId == matchId && item.UserId == userId && item.IsActive);
            if (!isParticipant)
            {
                throw new GameHubException(
                    GameHubErrorCodes.NotAuthorized,
                    "O usuário não é um participante ativo da partida.",
                    retryable: false);
            }
        }

        private long RequireUserId()
        {
            if (!AbpSession.UserId.HasValue)
            {
                throw new GameHubException(
                    GameHubErrorCodes.NotAuthenticated,
                    "Autenticação é obrigatória.",
                    retryable: false);
            }

            return AbpSession.UserId.Value;
        }

        private static GameInviteDto MapInvite(GameInvite invite)
        {
            return new GameInviteDto
            {
                InviteId = invite.Id,
                GameId = invite.GameId,
                MatchId = invite.MatchId,
                InviterUserId = invite.InviterUserId,
                InviteeUserId = invite.InviteeUserId,
                Status = invite.Status,
                ExpiresAt = invite.ExpiresAt
            };
        }

        private static GameNotificationDto MapNotification(GameNotification notification)
        {
            return new GameNotificationDto
            {
                Id = notification.Id,
                NotificationType = notification.NotificationType,
                Title = notification.Title,
                PayloadJson = notification.PayloadJson,
                IsRead = notification.IsRead,
                CreationTime = notification.CreationTime
            };
        }
    }
}
