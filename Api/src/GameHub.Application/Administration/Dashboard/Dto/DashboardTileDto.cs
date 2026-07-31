namespace GameHub.Administration.Dashboard.Dto
{
    /// <summary>
    /// Item de um tile do dashboard.
    /// </summary>
    public class DashboardTileDto
    {
        /// <summary>
        /// Identificador único do tile.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Título exibido no tile.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Valor numérico apresentado.
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// Cor/estilo do tile (primary, success, warning, danger, info).
        /// </summary>
        public string Style { get; set; }

        /// <summary>
        /// Ícone opcional do tile.
        /// </summary>
        public string Icon { get; set; }
    }
}
