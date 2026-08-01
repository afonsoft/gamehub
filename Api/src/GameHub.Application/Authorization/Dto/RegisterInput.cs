using System.ComponentModel.DataAnnotations;

namespace GameHub.Authorization.Dto
{
    /// <summary>
    /// Input for public user registration.
    /// </summary>
    public class RegisterInput
    {
        /// <summary>First name.</summary>
        [Required]
        [StringLength(Abp.Authorization.Users.AbpUserBase.MaxNameLength)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Last name.</summary>
        [Required]
        [StringLength(Abp.Authorization.Users.AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; } = string.Empty;

        /// <summary>Login username.</summary>
        [Required]
        [StringLength(Abp.Authorization.Users.AbpUserBase.MaxUserNameLength, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        /// <summary>Email address.</summary>
        [Required]
        [EmailAddress]
        [StringLength(Abp.Authorization.Users.AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>Password.</summary>
        [Required]
        [MinLength(6)]
        [StringLength(Abp.Authorization.Users.AbpUserBase.MaxPlainPasswordLength)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>Whether the user wants to register as a developer.</summary>
        public bool IsDeveloper { get; set; }

        /// <summary>How the user wants to associate with a tenant: PlayerDefault, CreateNew or JoinExisting.</summary>
        public string TenantSelectionMode { get; set; } = TenantSelectionModes.PlayerDefault;

        /// <summary>Name for the new tenant when TenantSelectionMode is CreateNew.</summary>
        [StringLength(128)]
        public string NewTenantName { get; set; }

        /// <summary>Tenant id to join when TenantSelectionMode is JoinExisting.</summary>
        public int? ExistingTenantId { get; set; }

        /// <summary>Optional message sent to tenant admins when requesting to join.</summary>
        [StringLength(1024)]
        public string JoinRequestMessage { get; set; }
    }

    public static class TenantSelectionModes
    {
        public const string PlayerDefault = "PlayerDefault";
        public const string CreateNew = "CreateNew";
        public const string JoinExisting = "JoinExisting";
    }
}
