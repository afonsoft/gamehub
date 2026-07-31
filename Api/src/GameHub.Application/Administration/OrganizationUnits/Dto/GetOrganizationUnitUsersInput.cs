using Abp.Application.Services.Dto;

namespace GameHub.Administration.OrganizationUnits.Dto
{
    /// <summary>
    /// Entrada para listar usuários ou perfis de uma unidade organizacional.
    /// </summary>
    public class GetOrganizationUnitUsersInput : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// Identificador da unidade organizacional.
        /// </summary>
        public long OrganizationUnitId { get; set; }

        /// <summary>
        /// Filtro por nome, e-mail ou nome de usuário.
        /// </summary>
        public string Filter { get; set; }
    }
}
