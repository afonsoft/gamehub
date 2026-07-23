using System;
using Abp.Application.Services.Dto;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Filtros e paginação para consulta de reports de usuários.
    /// </summary>
    public class GetReportsInput : PagedAndSortedResultRequestDto
    {
        /// <summary>Filtro por status do report.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Filtro por jogo reportado.</summary>
        public Guid? GameId { get; set; }
    }
}
