using System.Threading.Tasks;
using Abp.Domain.Services;

namespace GameHub.MultiTenancy
{
    public interface ITenantUserManager : IDomainService
    {
        /// <summary>
        /// Ensures a shadow user exists inside the target tenant for the given host user,
        /// creates the membership record and returns it.
        /// </summary>
        Task<UserTenantMembership> EnsureMembershipAsync(long hostUserId, int tenantId, bool isDefault = false);

        /// <summary>
        /// Removes the membership and deletes the shadow user inside the tenant.
        /// </summary>
        Task RemoveMembershipAsync(long hostUserId, int tenantId);
    }
}
