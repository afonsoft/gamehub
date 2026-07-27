using System.ComponentModel.DataAnnotations;

namespace GameHub.MultiTenancy.Dto
{
    public class GetUserTenantMembershipsInput
    {
        [Required]
        public long UserId { get; set; }
    }
}
