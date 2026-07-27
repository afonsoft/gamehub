using System.ComponentModel.DataAnnotations;

namespace GameHub.MultiTenancy.Dto
{
    public class SetDefaultTenantInput
    {
        [Required]
        public long UserId { get; set; }

        [Required]
        public int TenantId { get; set; }
    }
}
