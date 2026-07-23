using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Admin.Dto;

namespace GameHub.Admin
{
    /// <summary>
    /// Admin user list application service.
    /// </summary>
    public interface IAdminUserAppService : IApplicationService
    {
        /// <summary>
        /// Returns a paged list of platform users.
        /// </summary>
        Task<PagedResultDto<AdminUserListItemDto>> GetAllAsync(PagedAndSortedResultRequestDto input);
    }
}
