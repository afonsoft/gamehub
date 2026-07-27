using System.ComponentModel.DataAnnotations;

namespace GameHub.Companies.Dto
{
    public class RemoveEmployeeInput
    {
        [Required]
        public int TenantId { get; set; }

        [Required]
        public long UserId { get; set; }
    }
}
