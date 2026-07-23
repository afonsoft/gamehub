using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Eaf.Middleware.Authorization.Users;
using GameHub.Admin.Dto;
using GameHub.Authorization;
using GameHub.Developers;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Admin
{
    /// <summary>
    /// Lists users for the admin dashboard.
    /// </summary>
    [AbpAuthorize(GameHubPermissions.Pages_Users_Manage)]
    public class AdminUserAppService : GameHubAppServiceBase, IAdminUserAppService
    {
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<DeveloperProfile, Guid> _developerProfileRepository;

        public AdminUserAppService(
            IRepository<User, long> userRepository,
            IRepository<DeveloperProfile, Guid> developerProfileRepository)
        {
            _userRepository = userRepository;
            _developerProfileRepository = developerProfileRepository;
        }

        public async Task<PagedResultDto<AdminUserListItemDto>> GetAllAsync(PagedAndSortedResultRequestDto input)
        {
            var query = _userRepository.GetAll().Where(u => !u.IsDeleted);

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount > 0 ? input.MaxResultCount : 10)
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();
            var developerUserIds = await _developerProfileRepository.GetAll()
                .Where(p => userIds.Contains(p.UserId))
                .Select(p => p.UserId)
                .ToListAsync();

            var developerIdSet = new HashSet<long>(developerUserIds);

            var items = users.Select(u => new AdminUserListItemDto
            {
                Id = u.Id,
                UserName = u.UserName,
                EmailAddress = u.EmailAddress,
                FullName = $"{u.Name} {u.Surname}".Trim(),
                IsActive = u.IsActive,
                IsDeveloper = developerIdSet.Contains(u.Id),
                CreationTime = u.CreationTime,
            }).ToList();

            return new PagedResultDto<AdminUserListItemDto>(totalCount, items);
        }
    }
}
