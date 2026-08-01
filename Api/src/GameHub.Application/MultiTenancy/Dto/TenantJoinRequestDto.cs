namespace GameHub.MultiTenancy.Dto
{
    public class TenantJoinRequestDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string CreationTime { get; set; }
    }
}
