using System;
using GameHub.Developers;

namespace GameHub.Developer.Dto
{
    public class DeveloperTeamMemberDto
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DeveloperTeamRole Role { get; set; }
        public DateTime InvitedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public string InvitationToken { get; set; }
    }
}
