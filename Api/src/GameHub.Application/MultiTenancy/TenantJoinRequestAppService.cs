using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Data;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.UI;
using Eaf.Middleware.Authorization.Roles;
using Eaf.Middleware.Authorization.Users;
using GameHub.Authorization;
using GameHub.MultiTenancy.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Eaf.Middleware.Authorization.Roles.StaticRoleNames;

namespace GameHub.MultiTenancy
{
    [AbpAuthorize]
    public class TenantJoinRequestAppService : GameHubAppServiceBase, ITenantJoinRequestAppService
    {
        private readonly IRepository<TenantJoinRequest, long> _tenantJoinRequestRepository;
        private readonly IRepository<Eaf.Middleware.MultiTenancy.Tenant, int> _tenantRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<GameHub.MultiTenancy.UserTenantMembership, long> _membershipRepository;
        private readonly ITenantUserManager _tenantUserManager;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public TenantJoinRequestAppService(
            IRepository<TenantJoinRequest, long> tenantJoinRequestRepository,
            IRepository<Eaf.Middleware.MultiTenancy.Tenant, int> tenantRepository,
            IRepository<User, long> userRepository,
            IRepository<GameHub.MultiTenancy.UserTenantMembership, long> membershipRepository,
            ITenantUserManager tenantUserManager,
            UserManager userManager,
            RoleManager roleManager,
            IPasswordHasher<User> passwordHasher,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _tenantJoinRequestRepository = tenantJoinRequestRepository;
            _tenantRepository = tenantRepository;
            _userRepository = userRepository;
            _membershipRepository = membershipRepository;
            _tenantUserManager = tenantUserManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _passwordHasher = passwordHasher;
            _unitOfWorkManager = unitOfWorkManager;
        }

        [AbpAllowAnonymous]
        public virtual async Task<List<AvailableTenantDto>> GetAvailableTenantsAsync()
        {
            var tenants = await _tenantRepository.GetAll()
                .Where(t => t.IsActive && t.TenancyName != GameHubConsts.PlayerTenantName)
                .OrderBy(t => t.Name)
                .ToListAsync();

            return ObjectMapper.Map<List<AvailableTenantDto>>(tenants);
        }

        public virtual async Task<TenantJoinRequestDto> CreateRequestAsync(CreateTenantJoinRequestInput input)
        {
            var userId = AbpSession.UserId ?? throw new UserFriendlyException(L("UserNotLoggedIn"));

            await _tenantRepository.GetAsync(input.TenantId);

            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var existing = await _tenantJoinRequestRepository.FirstOrDefaultAsync(r =>
                    r.UserId == userId && r.TenantId == input.TenantId && r.Status == TenantJoinRequestStatus.Pending);
                if (existing != null)
                {
                    throw new UserFriendlyException(L("TenantJoinRequestAlreadyPending"));
                }

                var request = new TenantJoinRequest
                {
                    UserId = userId,
                    TenantId = input.TenantId,
                    Status = TenantJoinRequestStatus.Pending,
                    Message = input.Message,
                };

                await _tenantJoinRequestRepository.InsertAsync(request);
                await CurrentUnitOfWork.SaveChangesAsync();

                return await MapToDtoAsync(request);
            }
        }

        public virtual async Task<List<TenantJoinRequestDto>> GetMyRequestsAsync()
        {
            var userId = AbpSession.UserId ?? throw new UserFriendlyException(L("UserNotLoggedIn"));

            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var requests = await _tenantJoinRequestRepository.GetAll()
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.CreationTime)
                    .ToListAsync();

                return await MapToDtoListAsync(requests);
            }
        }

        [AbpAuthorize(GameHubPermissions.Pages_Users_Manage)]
        public virtual async Task<List<TenantJoinRequestDto>> GetPendingRequestsForCurrentTenantAsync()
        {
            var tenantId = AbpSession.TenantId ?? throw new UserFriendlyException(L("TenantRequired"));

            var requests = await _tenantJoinRequestRepository.GetAll()
                .Where(r => r.TenantId == tenantId && r.Status == TenantJoinRequestStatus.Pending)
                .OrderByDescending(r => r.CreationTime)
                .ToListAsync();

            return await MapToDtoListAsync(requests);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Users_Manage)]
        public virtual async Task<TenantJoinRequestDto> ApproveAsync(ApproveTenantJoinRequestInput input)
        {
            var tenantId = AbpSession.TenantId ?? throw new UserFriendlyException(L("TenantRequired"));

            TenantJoinRequest request;
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                request = await _tenantJoinRequestRepository.FirstOrDefaultAsync(r => r.Id == input.RequestId && r.TenantId == tenantId);
            }

            if (request == null)
                throw new UserFriendlyException(L("TenantJoinRequestNotFound"));

            if (request.Status != TenantJoinRequestStatus.Pending)
                throw new UserFriendlyException(L("TenantJoinRequestAlreadyProcessed"));

            request.Status = input.Approved ? TenantJoinRequestStatus.Approved : TenantJoinRequestStatus.Rejected;
            await _tenantJoinRequestRepository.UpdateAsync(request);

            if (input.Approved)
            {
                await ApproveMembershipAsync(request.UserId, tenantId);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            return await MapToDtoAsync(request);
        }

        private async Task ApproveMembershipAsync(long hostUserId, int tenantId)
        {
            GameHub.MultiTenancy.UserTenantMembership membership;
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                membership = await _membershipRepository.FirstOrDefaultAsync(m => m.UserId == hostUserId && m.TenantId == tenantId);
            }

            if (membership == null)
                throw new UserFriendlyException(L("TenantMembershipNotFound"));

            using (_unitOfWorkManager.Current.SetTenantId(tenantId))
            {
                var shadowUser = await _userRepository.GetAsync(membership.TenantUserId);
                if (!shadowUser.IsActive)
                {
                    shadowUser.IsActive = true;
                    (await _userManager.UpdateAsync(shadowUser)).CheckErrors();
                    await _unitOfWorkManager.Current.SaveChangesAsync();
                }

                if (!await _userManager.IsInRoleAsync(shadowUser, Tenants.User))
                {
                    (await _userManager.AddToRoleAsync(shadowUser, Tenants.User)).CheckErrors();
                }

                if (!await _userManager.IsInRoleAsync(shadowUser, GameHubRoleNames.Develop))
                {
                    (await _userManager.AddToRoleAsync(shadowUser, GameHubRoleNames.Develop)).CheckErrors();
                }
            }
        }

        private async Task<List<TenantJoinRequestDto>> MapToDtoListAsync(List<TenantJoinRequest> requests)
        {
            var result = new List<TenantJoinRequestDto>(requests.Count);
            foreach (var request in requests)
            {
                result.Add(await MapToDtoAsync(request));
            }

            return result;
        }

        private async Task<TenantJoinRequestDto> MapToDtoAsync(TenantJoinRequest request)
        {
            var dto = ObjectMapper.Map<TenantJoinRequestDto>(request);

            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == request.UserId);
                if (user != null)
                {
                    dto.UserName = user.UserName;
                    dto.UserFullName = $"{user.Name} {user.Surname}".Trim();
                }

                var tenant = await _tenantRepository.FirstOrDefaultAsync(t => t.Id == request.TenantId);
                if (tenant != null)
                {
                    dto.TenantName = tenant.Name;
                }
            }

            return dto;
        }

        public static class GameHubRoleNames
        {
            public const string Develop = "Develop";
        }
    }
}
