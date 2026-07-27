namespace GameHub.Web.Models.HubAuth
{
    public class SelectTenantResult
    {
        public string AccessToken { get; set; }
        public int ExpireInSeconds { get; set; }
        public long UserId { get; set; }
        public int TenantId { get; set; }
    }
}
