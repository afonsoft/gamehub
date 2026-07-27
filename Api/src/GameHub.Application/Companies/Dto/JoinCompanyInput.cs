using System.ComponentModel.DataAnnotations;

namespace GameHub.Companies.Dto
{
    public class JoinCompanyInput
    {
        [Required]
        [StringLength(128)]
        public string TenancyName { get; set; }

        [Required]
        [StringLength(32)]
        public string UserName { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [Required]
        [StringLength(128)]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string EmailAddress { get; set; }

        [Required]
        [StringLength(128, MinimumLength = 6)]
        public string Password { get; set; }

        [StringLength(32)]
        public string Role { get; set; } = "Developer";
    }
}
