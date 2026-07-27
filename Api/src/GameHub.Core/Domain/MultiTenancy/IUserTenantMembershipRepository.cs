using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;

namespace GameHub.MultiTenancy
{
    public interface IUserTenantMembershipRepository : IRepository<UserTenantMembership, long>
    {
        Task<UserTenantMembership> GetByUserAndTenantAsync(long userId, int tenantId);
        Task<List<UserTenantMembership>> GetAllByUserAsync(long userId);
        Task<UserTenantMembership> GetDefaultByUserAsync(long userId);
        Task<bool> ExistsAsync(long userId, int tenantId);
    }
}
