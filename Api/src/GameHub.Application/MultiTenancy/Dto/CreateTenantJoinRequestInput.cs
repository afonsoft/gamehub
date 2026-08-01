using System.ComponentModel.DataAnnotations;

namespace GameHub.MultiTenancy.Dto
{
    public class CreateTenantJoinRequestInput
    {
        [Required]
        public int TenantId { get; set; }

        [StringLength(1024)]
        public string Message { get; set; }
    }
}
