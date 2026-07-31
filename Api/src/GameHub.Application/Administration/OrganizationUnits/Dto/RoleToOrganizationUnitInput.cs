namespace GameHub.Administration.OrganizationUnits.Dto
{
    /// <summary>
    /// Entrada para adicionar/remover um perfil de uma unidade organizacional.
    /// </summary>
    public class RoleToOrganizationUnitInput
    {
        /// <summary>
        /// Identificador da unidade organizacional.
        /// </summary>
        public long OrganizationUnitId { get; set; }

        /// <summary>
        /// Identificador do perfil.
        /// </summary>
        public int RoleId { get; set; }
    }
}
