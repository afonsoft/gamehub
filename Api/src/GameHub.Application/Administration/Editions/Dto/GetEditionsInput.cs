using Abp.Application.Services.Dto;

namespace GameHub.Administration.Editions.Dto
{
    /// <summary>
    /// Entrada para consulta paginada de Editions.
    /// </summary>
    public class GetEditionsInput : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// Filtro por nome da edição.
        /// </summary>
        public string Filter { get; set; }
    }
}
