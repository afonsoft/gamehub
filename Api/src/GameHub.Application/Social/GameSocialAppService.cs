using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Timing;
using GameHub.Multiplayer;
using GameHub.Moderation;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Social
{
    /// <summary>
    /// Implements authenticated social actions without exposing internal user data.
    /// </summary>
    public class GameSocialAppService : GameHubAppServiceBase, IGameSocialAppService
    {
        private readonly IRepository<GameInvite, Guid> _inviteRepository;
        private readonly IRepository<GameNotification, Guid> _notificationRepository;
        private readonly IRepository<MatchState, Guid> _matchRepository;
        private readonly IRepository<MatchParticipant, Guid> _participantRepository;
        private readonly IMultiplayerPresenceStore _presenceStore;
        private readonly IRepository<UserReport, Guid> _reportRepository;

        public GameSocialAppService(
            IRepository<GameInvite, Guid> inviteRepository,
            IRepository<GameNotification, Guid> notificationRepository,
            IRepository<MatchState, Guid> matchRepository,
            IRepository<MatchParticipant, Guid> participantRepository,
            IMultiplayerPresenceStore presenceStore,
            IRepository<UserReport, Guid> reportRepository)
        {
            _inviteRepository = inviteRepository;
            _notificationRepository = notificationRepository;
            _matchRepository = matchRepository;
            _participantRepository = participantRepository;
            _presenceStore = presenceStore;
            _reportRepository = reportRepository;
        }

        public async Task<GameInviteDto> InvitePlayerAsync(InvitePlayerInput input)
        {
            var inviterUserId = RequireUserId();
            var match = await _matchRepository.GetAll()
                .FirstOrDefaultAsync(item => item.Id == input.MatchId && item.GameId == input.GameId);

            if (match == null || match.Status == MatchStatus.Ended || match.Status == MatchStatus.Cancelled)
            {
                throw new InvalidOperationException("Match is not available.");
            }

            await EnsureActiveParticipantAsync(input.MatchId, inviterUserId);
            await UserManager.GetUserByIdAsync(input.InviteeUserId);

            var expiresAt = input.ExpiresAt ?? Clock.Now.AddMinutes(15);
            if (expiresAt <= Clock.Now)
            {
                throw new ArgumentException("Invite expiration must be in the future.", nameof(input));
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
                Id = Guid.NewGuid(),
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
                Id = Guid.NewGuid(),
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
                throw new InvalidOperationException("Invite is not available.");
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
                throw new ArgumentException("A different player must be reported.", nameof(input));
            }

            if (string.IsNullOrWhiteSpace(input.Reason) || input.Reason.Length > 128)
            {
                throw new ArgumentException("A valid report reason is required.", nameof(input));
            }

            await _reportRepository.InsertAsync(new UserReport
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                UserId = input.ReportedUserId,
                Reason = input.Reason,
                Status = UserReportStatus.Open,
                CreationTime = Clock.Now
            });
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private async Task EnsureActiveParticipantAsync(Guid matchId, long userId)
        {
            var isParticipant = await _participantRepository.GetAll()
                .AnyAsync(item => item.MatchId == matchId && item.UserId == userId && item.IsActive);
            if (!isParticipant)
            {
                throw new InvalidOperationException("User is not an active match participant.");
            }
        }

        private long RequireUserId()
        {
            if (!AbpSession.UserId.HasValue)
            {
                throw new InvalidOperationException("Authentication is required.");
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
