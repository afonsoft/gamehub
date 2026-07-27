using System.ComponentModel.DataAnnotations;

namespace GameHub.Companies.Dto
{
    public class InviteEmployeeInput
    {
        [Required]
        public int TenantId { get; set; }

        [Required]
        [StringLength(256)]
        public string EmailOrUserName { get; set; }

        [Required]
        [StringLength(64)]
        public string Role { get; set; }

        public bool IsDefault { get; set; }
    }
}
