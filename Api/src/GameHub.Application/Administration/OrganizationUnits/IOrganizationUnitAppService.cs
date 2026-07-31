using Abp.Application.Services.Dto;
using GameHub.Administration.OrganizationUnits.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameHub.Administration.OrganizationUnits
{
    /// <summary>
    /// Serviço de aplicação para gerenciamento de unidades organizacionais.
    /// </summary>
    public interface IOrganizationUnitAppService
    {
        /// <summary>
        /// Obtém todas as unidades organizacionais em estrutura de árvore.
        /// </summary>
        Task<ListResultDto<OrganizationUnitDto>> GetOrganizationUnits();

        /// <summary>
        /// Cria uma nova unidade organizacional.
        /// </summary>
        Task<OrganizationUnitDto> CreateAsync(CreateOrganizationUnitInput input);

        /// <summary>
        /// Atualiza o nome de uma unidade organizacional.
        /// </summary>
        Task<OrganizationUnitDto> UpdateAsync(UpdateOrganizationUnitInput input);

        /// <summary>
        /// Move uma unidade organizacional para outro pai.
        /// </summary>
        Task MoveAsync(MoveOrganizationUnitInput input);

        /// <summary>
        /// Remove uma unidade organizacional.
        /// </summary>
        Task DeleteAsync(EntityDto<long> input);

        /// <summary>
        /// Obtém os usuários de uma unidade organizacional.
        /// </summary>
        Task<PagedResultDto<OrganizationUnitUserListDto>> GetOrganizationUnitUsersAsync(GetOrganizationUnitUsersInput input);

        /// <summary>
        /// Adiciona um usuário à unidade organizacional.
        /// </summary>
        Task AddUserToOrganizationUnit(UserToOrganizationUnitInput input);

        /// <summary>
        /// Remove um usuário da unidade organizacional.
        /// </summary>
        Task RemoveUserFromOrganizationUnit(UserToOrganizationUnitInput input);

        /// <summary>
        /// Obtém os perfis de uma unidade organizacional.
        /// </summary>
        Task<PagedResultDto<OrganizationUnitRoleListDto>> GetOrganizationUnitRolesAsync(GetOrganizationUnitUsersInput input);

        /// <summary>
        /// Adiciona um perfil à unidade organizacional.
        /// </summary>
        Task AddRoleToOrganizationUnit(RoleToOrganizationUnitInput input);

        /// <summary>
        /// Remove um perfil da unidade organizacional.
        /// </summary>
        Task RemoveRoleFromOrganizationUnit(RoleToOrganizationUnitInput input);
    }
}
