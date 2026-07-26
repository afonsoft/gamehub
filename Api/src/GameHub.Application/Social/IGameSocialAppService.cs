using System;
using System.Threading.Tasks;

namespace GameHub.Social
{
    /// <summary>
    /// Provides authenticated game-scoped social capabilities.
    /// </summary>
    public interface IGameSocialAppService
    {
        /// <summary>Sends a match invite to an active participant.</summary>
        Task<GameInviteDto> InvitePlayerAsync(InvitePlayerInput input);

        /// <summary>Accepts an unexpired invite addressed to the current user.</summary>
        Task<GameInviteDto> AcceptInviteAsync(Guid inviteId);

        /// <summary>Gets recent notifications for the current user.</summary>
        Task<GameNotificationListDto> GetNotificationsAsync();

        /// <summary>Marks an owned notification as read.</summary>
        Task MarkNotificationReadAsync(Guid notificationId);

        /// <summary>Gets coarse presence for a user in the current tenant.</summary>
        Task<PresenceDto> GetPresenceAsync(long userId);

        /// <summary>Creates a moderation report for another player.</summary>
        Task ReportPlayerAsync(ReportPlayerInput input);
    }
}
