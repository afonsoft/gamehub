using System;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// KPIs exibidos no dashboard administrativo.
    /// </summary>
    public class AdminDashboardSummaryDto
    {
        /// <summary>Total de jogos cadastrados.</summary>
        public long TotalGames { get; set; }

        /// <summary>Total de revisões pendentes.</summary>
        public long PendingReviews { get; set; }

        /// <summary>Total acumulado de plays.</summary>
        public long TotalPlays { get; set; }

        /// <summary>Usuários ativos nos últimos 7 dias.</summary>
        public long ActiveUsers7d { get; set; }

        /// <summary>Total de usuários cadastrados.</summary>
        public long TotalUsers { get; set; }

        /// <summary>Total de desenvolvedores cadastrados.</summary>
        public long TotalDevelopers { get; set; }

        /// <summary>Total de builds (uploads) enviados.</summary>
        public long TotalBuilds { get; set; }

        /// <summary>Builds aguardando aprovação do desenvolvedor.</summary>
        public long PendingUploads { get; set; }
    }
}
