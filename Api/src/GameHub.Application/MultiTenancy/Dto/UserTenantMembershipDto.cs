namespace GameHub.MultiTenancy.Dto
{
    public class UserTenantMembershipDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string TenantTenancyName { get; set; }
        public long TenantUserId { get; set; }
        public bool IsDefault { get; set; }
    }
}
