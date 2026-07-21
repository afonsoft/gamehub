using System.Collections.Generic;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Série temporal de plays para o gráfico do dashboard.
    /// </summary>
    public class PlaysOverTimeResultDto
    {
        /// <summary>Dias com total de plays.</summary>
        public List<PlaysOverTimeItemDto> Items { get; set; } = new List<PlaysOverTimeItemDto>();
    }
}
