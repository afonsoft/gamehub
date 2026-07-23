using System;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// User list item for the admin dashboard.
    /// </summary>
    public class AdminUserListItemDto
    {
        /// <summary>User identifier.</summary>
        public long Id { get; set; }

        /// <summary>Username.</summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>Email address.</summary>
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>Full name.</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Whether the user is active.</summary>
        public bool IsActive { get; set; }

        /// <summary>Whether the user has a developer profile.</summary>
        public bool IsDeveloper { get; set; }

        /// <summary>Account creation time.</summary>
        public DateTime CreationTime { get; set; }
    }
}
