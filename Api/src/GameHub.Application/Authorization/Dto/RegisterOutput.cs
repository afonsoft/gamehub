namespace GameHub.Authorization.Dto
{
    /// <summary>
    /// Result of a successful user registration.
    /// </summary>
    public class RegisterOutput
    {
        /// <summary>Registered user identifier.</summary>
        public long UserId { get; set; }

        /// <summary>Registered username.</summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>Tenancy name of the tenant created or joined, when applicable.</summary>
        public string TenancyName { get; set; } = string.Empty;

        /// <summary>Identifier of the tenant created or joined, when applicable.</summary>
        public int? TenantId { get; set; }

        /// <summary>Indicates whether the user can log in immediately or must wait for approval.</summary>
        public bool CanLogin { get; set; }
    }
}
