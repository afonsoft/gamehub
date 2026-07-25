using System.ComponentModel.DataAnnotations;
using GameHub.Developers;

namespace GameHub.Developer.Dto
{
    public class InviteMemberInput
    {
        [Required]
        [StringLength(256)]
        public string Email { get; set; }

        public DeveloperTeamRole Role { get; set; } = DeveloperTeamRole.Developer;
    }
}
