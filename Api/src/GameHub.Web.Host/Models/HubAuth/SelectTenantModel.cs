namespace GameHub.Web.Models.HubAuth
{
    public class SelectTenantModel
    {
        public string UserNameOrEmailAddress { get; set; }
        public string Password { get; set; }
        public int TenantId { get; set; }
    }
}
