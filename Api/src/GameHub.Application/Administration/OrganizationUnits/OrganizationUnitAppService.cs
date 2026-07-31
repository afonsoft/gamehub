using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Organizations;
using Eaf.Middleware.Authorization.Roles;
using Eaf.Middleware.Authorization.Users;
using GameHub.Administration.OrganizationUnits.Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace GameHub.Administration.OrganizationUnits
{
    /// <summary>
    /// Serviço de aplicação para gerenciamento de unidades organizacionais.
    /// </summary>
    [AbpAuthorize("Pages.Administration.OrganizationUnits")]
    public class OrganizationUnitAppService : GameHubAppServiceBase, IOrganizationUnitAppService
    {
        private readonly IOrganizationUnitManager _organizationUnitManager;
        private readonly IRepository<OrganizationUnit, long> _organizationUnitRepository;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;

        /// <summary>
        /// OrganizationUnitAppService.
        /// </summary>
        public OrganizationUnitAppService(
            IOrganizationUnitManager organizationUnitManager,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            UserManager userManager,
            RoleManager roleManager)
        {
            _organizationUnitManager = organizationUnitManager;
            _organizationUnitRepository = organizationUnitRepository;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Obtém todas as unidades organizacionais em estrutura de árvore.
        /// </summary>
        public virtual async Task<ListResultDto<OrganizationUnitDto>> GetOrganizationUnits()
        {
            var query = await _organizationUnitRepository.GetAllListAsync();
            var lookup = query.ToDictionary(ou => ou.Id, ou => ObjectMapper.Map<OrganizationUnitDto>(ou));

            var rootItems = new List<OrganizationUnitDto>();
            foreach (var item in lookup.Values)
            {
                if (item.ParentId.HasValue && lookup.TryGetValue(item.ParentId.Value, out var parent))
                {
                    parent.Children ??= new List<OrganizationUnitDto>();
                    parent.Children.Add(item);
                }
                else
                {
                    rootItems.Add(item);
                }
            }

            return new ListResultDto<OrganizationUnitDto>(rootItems);
        }

        /// <summary>
        /// Cria uma nova unidade organizacional.
        /// </summary>
        [AbpAuthorize("Pages.Administration.OrganizationUnits.Create")]
        public virtual async Task<OrganizationUnitDto> CreateAsync(CreateOrganizationUnitInput input)
        {
            var organizationUnit = new OrganizationUnit
            {
                DisplayName = input.DisplayName,
                ParentId = input.ParentId
            };
            await _organizationUnitManager.CreateAsync(organizationUnit);
            return ObjectMapper.Map<OrganizationUnitDto>(organizationUnit);
        }

        /// <summary>
        /// Atualiza o nome de uma unidade organizacional.
        /// </summary>
        [AbpAuthorize("Pages.Administration.OrganizationUnits.Edit")]
        public virtual async Task<OrganizationUnitDto> UpdateAsync(UpdateOrganizationUnitInput input)
        {
            var organizationUnit = await _organizationUnitRepository.GetAsync(input.Id);
            organizationUnit.DisplayName = input.DisplayName;
            await _organizationUnitManager.UpdateAsync(organizationUnit);
            return ObjectMapper.Map<OrganizationUnitDto>(organizationUnit);
        }

        /// <summary>
        /// Move uma unidade organizacional para outro pai.
        /// </summary>
        [AbpAuthorize("Pages.Administration.OrganizationUnits.Edit")]
        public virtual async Task MoveAsync(MoveOrganizationUnitInput input)
        {
            await _organizationUnitManager.MoveAsync(input.Id, input.NewParentId);
        }

        /// <summary>
        /// Remove uma unidade organizacional.
        /// </summary>
        [AbpAuthorize("Pages.Administration.OrganizationUnits.Delete")]
        public virtual async Task DeleteAsync(EntityDto<long> input)
        {
            await _organizationUnitManager.DeleteAsync(input.Id);
        }

        /// <summary>
        /// Obtém os usuários de uma unidade organizacional.
        /// </summary>
        public virtual async Task<PagedResultDto<OrganizationUnitUserListDto>> GetOrganizationUnitUsersAsync(GetOrganizationUnitUsersInput input)
        {
            var organizationUnit = await _organizationUnitRepository.GetAsync(input.OrganizationUnitId);
            var users = await _userManager.GetUsersInOrganizationUnitAsync(organizationUnit, false);

            var query = users.AsQueryable()
                .WhereIf(!input.Filter.IsNullOrWhiteSpace(), u =>
                    u.Name.Contains(input.Filter) ||
                    u.Surname.Contains(input.Filter) ||
                    u.UserName.Contains(input.Filter) ||
                    u.EmailAddress.Contains(input.Filter));

            var total = await query.CountAsync();
            var ordered = DynamicQueryableExtensions.OrderBy(query, input.Sorting ?? "Name");
            var items = await ordered.PageBy(input).ToListAsync();

            return new PagedResultDto<OrganizationUnitUserListDto>(total, ObjectMapper.Map<List<OrganizationUnitUserListDto>>(items));
        }

        /// <summary>
        /// Adiciona um usuário à unidade organizacional.
        /// </summary>
        [AbpAuthorize("Pages.Administration.OrganizationUnits.ManageMembers")]
        public virtual async Task AddUserToOrganizationUnit(UserToOrganizationUnitInput input)
        {
            await _userManager.AddToOrganizationUnitAsync(input.UserId, input.OrganizationUnitId);
        }

        /// <summary>
        /// Remove um usuário da unidade organizacional.
        /// </summary>
        [AbpAuthorize("Pages.Administration.OrganizationUnits.ManageMembers")]
        public virtual async Task RemoveUserFromOrganizationUnit(UserToOrganizationUnitInput input)
        {
            await _userManager.RemoveFromOrganizationUnitAsync(input.UserId, input.OrganizationUnitId);
        }

        /// <summary>
        /// Obtém os perfis de uma unidade organizacional.
        /// </summary>
        public virtual async Task<PagedResultDto<OrganizationUnitRoleListDto>> GetOrganizationUnitRolesAsync(GetOrganizationUnitUsersInput input)
        {
            var organizationUnit = await _organizationUnitRepository.GetAsync(input.OrganizationUnitId);
            var roles = await _roleManager.GetRolesInOrganizationUnitAsync(organizationUnit, false);

            var query = roles.AsQueryable()
                .WhereIf(!input.Filter.IsNullOrWhiteSpace(), r =>
                    r.Name.Contains(input.Filter) ||
                    r.DisplayName.Contains(input.Filter));

            var total = await query.CountAsync();
            var ordered = DynamicQueryableExtensions.OrderBy(query, input.Sorting ?? "Name");
            var items = await ordered.PageBy(input).ToListAsync();

            return new PagedResultDto<OrganizationUnitRoleListDto>(total, ObjectMapper.Map<List<OrganizationUnitRoleListDto>>(items));
        }

        /// <summary>
        /// Adiciona um perfil à unidade organizacional.
        /// </summary>
        [AbpAuthorize("Pages.Administration.OrganizationUnits.ManageRoles")]
        public virtual async Task AddRoleToOrganizationUnit(RoleToOrganizationUnitInput input)
        {
            await _roleManager.AddToOrganizationUnitAsync(input.RoleId, input.OrganizationUnitId, AbpSession.TenantId);
        }

        /// <summary>
        /// Remove um perfil da unidade organizacional.
        /// </summary>
        [AbpAuthorize("Pages.Administration.OrganizationUnits.ManageRoles")]
        public virtual async Task RemoveRoleFromOrganizationUnit(RoleToOrganizationUnitInput input)
        {
            await _roleManager.RemoveFromOrganizationUnitAsync(input.RoleId, input.OrganizationUnitId);
        }
    }
}
