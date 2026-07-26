using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace GameHub.Social
{
    /// <summary>Input for inviting a player to a match.</summary>
    public class InvitePlayerInput
    {
        public Guid GameId { get; set; }

        public Guid MatchId { get; set; }

        public long InviteeUserId { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>Safe representation of a match invite.</summary>
    public class GameInviteDto
    {
        public Guid InviteId { get; set; }

        public Guid GameId { get; set; }

        public Guid MatchId { get; set; }

        public long InviterUserId { get; set; }

        public long InviteeUserId { get; set; }

        public string Status { get; set; }

        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>Safe notification exposed to the game SDK.</summary>
    public class GameNotificationDto
    {
        public Guid Id { get; set; }

        public string NotificationType { get; set; }

        public string Title { get; set; }

        public string PayloadJson { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreationTime { get; set; }
    }

    /// <summary>Paginated notification response.</summary>
    public class GameNotificationListDto : ListResultDto<GameNotificationDto>
    {
        public GameNotificationListDto(IReadOnlyList<GameNotificationDto> items)
            : base(items)
        {
        }
    }

    /// <summary>Coarse online/offline presence.</summary>
    public class PresenceDto
    {
        public long UserId { get; set; }

        public string State { get; set; }
    }

    /// <summary>Input for reporting another player.</summary>
    public class ReportPlayerInput
    {
        public Guid GameId { get; set; }

        public long ReportedUserId { get; set; }

        public string Reason { get; set; }
    }
}
