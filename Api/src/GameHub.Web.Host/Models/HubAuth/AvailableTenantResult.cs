namespace GameHub.Web.Models.HubAuth
{
    public class AvailableTenantResult
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string TenancyName { get; set; }
        public bool IsDefault { get; set; }
    }
}
