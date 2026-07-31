using System.Collections.Generic;

namespace GameHub.Administration.Dashboard.Dto
{
    /// <summary>
    /// Dados agregados apresentados no dashboard.
    /// </summary>
    public class DashboardOutput
    {
        /// <summary>
        /// Conjunto de tiles do dashboard.
        /// </summary>
        public List<DashboardTileDto> Tiles { get; set; }

        /// <summary>
        /// Indica se o dashboard é do host.
        /// </summary>
        public bool IsHostDashboard { get; set; }

        /// <summary>
        /// Construtor padrão.
        /// </summary>
        public DashboardOutput()
        {
            Tiles = new List<DashboardTileDto>();
        }
    }
}
