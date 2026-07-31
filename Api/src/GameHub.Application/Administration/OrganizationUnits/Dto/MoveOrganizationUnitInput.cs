namespace GameHub.Administration.OrganizationUnits.Dto
{
    /// <summary>
    /// Entrada para mover uma unidade organizacional na árvore.
    /// </summary>
    public class MoveOrganizationUnitInput
    {
        /// <summary>
        /// Identificador da unidade organizacional a ser movida.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Identificador da nova unidade pai. Nulo para raiz.
        /// </summary>
        public long? NewParentId { get; set; }
    }
}
