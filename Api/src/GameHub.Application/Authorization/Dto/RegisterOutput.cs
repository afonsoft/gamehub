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
    }
}
