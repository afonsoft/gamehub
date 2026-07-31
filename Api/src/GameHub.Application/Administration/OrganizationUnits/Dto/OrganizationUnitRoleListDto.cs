namespace GameHub.Administration.OrganizationUnits.Dto
{
    /// <summary>
    /// DTO para representar um perfil em uma unidade organizacional.
    /// </summary>
    public class OrganizationUnitRoleListDto
    {
        /// <summary>
        /// Identificador do perfil.
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Nome do perfil.
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Nome de exibição do perfil.
        /// </summary>
        public string RoleDisplayName { get; set; }
    }
}
