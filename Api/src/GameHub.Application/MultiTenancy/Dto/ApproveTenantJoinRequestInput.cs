using System.ComponentModel.DataAnnotations;

namespace GameHub.MultiTenancy.Dto
{
    public class ApproveTenantJoinRequestInput
    {
        [Required]
        public long RequestId { get; set; }

        public bool Approved { get; set; } = true;
    }
}
