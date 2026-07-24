using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using Eaf.Middleware.Authorization.Users;

namespace GameHub.Player
{
    /// <summary>
    /// Player preferences such as language and accessibility settings.
    /// </summary>
    public class PlayerPreference : Entity<Guid>, IMayHaveTenant, IHasCreationTime
    {
        public int? TenantId { get; set; }

        public long UserId { get; set; }

        [StringLength(16)]
        public string Language { get; set; } = "en-US";

        public DateTime CreationTime { get; set; }

        public DateTime? LastModificationTime { get; set; }

        public virtual User User { get; set; }
    }
}
