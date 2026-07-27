using System.ComponentModel.DataAnnotations;

namespace GameHub.Companies.Dto
{
    public class SetDefaultEmployeeInput
    {
        [Required]
        public int TenantId { get; set; }

        [Required]
        public long UserId { get; set; }
    }
}
