using Abp.Application.Services.Dto;
using System;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Filtros e paginação para consulta de logs de auditoria.
    /// </summary>
    public class GetAuditLogsInput : PagedAndSortedResultRequestDto
    {
        /// <summary>Data/hora inicial.</summary>
        public DateTime? StartTime { get; set; }

        /// <summary>Data/hora final.</summary>
        public DateTime? EndTime { get; set; }

        /// <summary>Filtro por nome do serviço ou método.</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>Filtro por nome de usuário.</summary>
        public string UserName { get; set; } = string.Empty;
    }
}
