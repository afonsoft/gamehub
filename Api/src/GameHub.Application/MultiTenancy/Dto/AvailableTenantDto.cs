namespace GameHub.MultiTenancy.Dto
{
    public class AvailableTenantDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TenancyName { get; set; }
        public bool IsActive { get; set; }
    }
}
