using System;
using Abp.Application.Services.Dto;

namespace GameHub.Moderation.Dto
{
    public class UserContentDto : EntityDto<Guid>
    {
        public Guid GameId { get; set; }
        public long? UserId { get; set; }
        public UserContentType ContentType { get; set; }
        public string Text { get; set; }
        public int? Rating { get; set; }
        public bool IsApproved { get; set; }
        public bool RequiresModeration { get; set; }
        public string ModerationReason { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
