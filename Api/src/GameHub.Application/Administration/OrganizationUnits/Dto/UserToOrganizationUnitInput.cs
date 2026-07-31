namespace GameHub.Administration.OrganizationUnits.Dto
{
    /// <summary>
    /// Entrada para adicionar/remover um usuário de uma unidade organizacional.
    /// </summary>
    public class UserToOrganizationUnitInput
    {
        /// <summary>
        /// Identificador da unidade organizacional.
        /// </summary>
        public long OrganizationUnitId { get; set; }

        /// <summary>
        /// Identificador do usuário.
        /// </summary>
        public long UserId { get; set; }
    }
}
