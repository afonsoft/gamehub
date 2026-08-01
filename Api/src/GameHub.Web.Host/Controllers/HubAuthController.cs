using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Security;
using Abp.UI;
using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.Web.Authentication;
using Eaf.Middleware.Web.Controllers;
using GameHub.MultiTenancy;
using GameHub.Web.Models.HubAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Web.Controllers
{
    /// <summary>
    /// Public hub authentication endpoints supporting multi-tenant user login.
    /// </summary>
    [Route("api/hub/auth")]
    [ApiController]
    [AllowAnonymous]
    public class HubAuthController : MiddlewareControllerBase
    {
        private static readonly TimeSpan AccessTokenExpiration = TimeSpan.FromDays(1);

        private readonly ITokenAuthenticationService _tokenAuthenticationService;
        private readonly UserManager _userManager;
        private readonly IRepository<Eaf.Middleware.MultiTenancy.UserTenantMembership, long> _membershipRepository;
        private readonly IRepository<Eaf.Middleware.MultiTenancy.Tenant, int> _tenantRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public HubAuthController(
            ITokenAuthenticationService tokenAuthenticationService,
            UserManager userManager,
            IRepository<Eaf.Middleware.MultiTenancy.UserTenantMembership, long> membershipRepository,
            IRepository<Eaf.Middleware.MultiTenancy.Tenant, int> tenantRepository,
            IRepository<User, long> userRepository,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _tokenAuthenticationService = tokenAuthenticationService;
            _userManager = userManager;
            _membershipRepository = membershipRepository;
            _tenantRepository = tenantRepository;
            _userRepository = userRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        /// <summary>
        /// Authenticates the host user and returns the list of tenants it can access.
        /// </summary>
        [HttpPost("available-tenants")]
        [ProducesResponseType(typeof(List<HubAvailableTenantResult>), 200)]
        public virtual async Task<IActionResult> GetAvailableTenants([FromBody] HubAvailableTenantsModel model)
        {
            using (var uow = _unitOfWorkManager.Begin())
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var user = await FindUserAsync(model.UserNameOrEmailAddress);
                if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                    throw new UserFriendlyException(L("InvalidUserNameOrPassword"));

                List<HubAvailableTenantResult> result;
                if (user.TenantId.HasValue)
                {
                    var tenant = await _tenantRepository.GetAsync(user.TenantId.Value);
                    result = new List<HubAvailableTenantResult>
                    {
                        new()
                        {
                            TenantId = tenant.Id,
                            TenantName = tenant.Name,
                            TenancyName = tenant.TenancyName,
                            IsDefault = true,
                        }
                    };
                }
                else
                {
                    var memberships = await _membershipRepository.GetAllListAsync(m => m.UserId == user.Id);
                    if (memberships.Count == 0)
                    {
                        await uow.CompleteAsync();
                        return Ok(new List<HubAvailableTenantResult>());
                    }

                    var tenantIds = memberships.Select(m => m.TenantId).Distinct().ToList();
                    var tenants = await _tenantRepository.GetAllListAsync(t => tenantIds.Contains(t.Id));

                    result = memberships
                        .Select(m =>
                        {
                            var tenant = tenants.FirstOrDefault(t => t.Id == m.TenantId);
                            return new HubAvailableTenantResult
                            {
                                TenantId = m.TenantId,
                                TenantName = tenant?.Name,
                                TenancyName = tenant?.TenancyName,
                                IsDefault = m.IsDefault,
                            };
                        })
                        .ToList();
                }

                await uow.CompleteAsync();
                return Ok(result);
            }
        }

        /// <summary>
        /// Selects a tenant for the authenticated host user and issues an access token for the tenant-level user.
        /// </summary>
        [HttpPost("select-tenant")]
        [ProducesResponseType(typeof(SelectTenantResult), 200)]
        public virtual async Task<IActionResult> SelectTenant([FromBody] HubSelectTenantModel model)
        {
            using (var uow = _unitOfWorkManager.Begin())
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var user = await FindUserAsync(model.UserNameOrEmailAddress);
                if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                    throw new UserFriendlyException(L("InvalidUserNameOrPassword"));

                User shadowUser;
                if (user.TenantId.HasValue)
                {
                    if (user.TenantId.Value != model.TenantId)
                        throw new UserFriendlyException(L("UserIsNotAssociatedWithSelectedTenant"));

                    shadowUser = user;
                }
                else
                {
                    var membership = await _membershipRepository.FirstOrDefaultAsync(m =>
                        m.UserId == user.Id && m.TenantId == model.TenantId);

                    if (membership == null)
                        throw new UserFriendlyException(L("UserIsNotAssociatedWithSelectedTenant"));

                    using (_unitOfWorkManager.Current.SetTenantId(model.TenantId))
                    {
                        shadowUser = await _userRepository.GetAsync(membership.TenantUserId);
                        if (!shadowUser.IsActive)
                            throw new UserFriendlyException(L("UserIsNotActiveAndCanNotLogin"));
                    }
                }

                var token = await CreateAccessTokenForUserAsync(shadowUser, model.TenantId);
                await uow.CompleteAsync();

                return Ok(new SelectTenantResult
                {
                    AccessToken = token,
                    ExpireInSeconds = (int)AccessTokenExpiration.TotalSeconds,
                    UserId = shadowUser.Id,
                    TenantId = model.TenantId,
                });
            }
        }

        private async Task<User> FindUserAsync(string userNameOrEmailAddress)
        {
            return await _userRepository.GetAll()
                .Where(u => u.UserName == userNameOrEmailAddress || u.EmailAddress == userNameOrEmailAddress)
                .OrderBy(u => u.TenantId == null ? 0 : 1)
                .FirstOrDefaultAsync();
        }

        private async Task<string> CreateAccessTokenForUserAsync(User user, int tenantId)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(AbpClaimTypes.UserId, user.Id.ToString()),
                new(AbpClaimTypes.UserName, user.UserName),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(AbpClaimTypes.TenantId, tenantId.ToString()),
                new("tenantid", tenantId.ToString()),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim(AbpClaimTypes.Role, role));
            }

            return await _tokenAuthenticationService.CreateAccessTokenAsync(claims, AccessTokenExpiration);
        }
    }
}
