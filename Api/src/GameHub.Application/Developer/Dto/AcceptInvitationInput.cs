using System.ComponentModel.DataAnnotations;

namespace GameHub.Developer.Dto
{
    public class AcceptInvitationInput
    {
        [Required]
        [StringLength(128)]
        public string Token { get; set; }
    }
}
