using System;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Entrada do log de auditoria para o painel administrativo.
    /// </summary>
    public class AuditLogDto
    {
        /// <summary>Identificador do log.</summary>
        public long Id { get; set; }

        /// <summary>Horário da execução.</summary>
        public DateTime ExecutionTime { get; set; }

        /// <summary>Identificador do usuário.</summary>
        public long? UserId { get; set; }

        /// <summary>Nome do usuário que executou a ação.</summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>Nome do serviço + método executado.</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>Detalhes da requisição.</summary>
        public string Details { get; set; } = string.Empty;

        /// <summary>Duração em milissegundos.</summary>
        public int ExecutionDuration { get; set; }

        /// <summary>Endereço IP do cliente.</summary>
        public string ClientIpAddress { get; set; } = string.Empty;

        /// <summary>Informações do navegador.</summary>
        public string BrowserInfo { get; set; } = string.Empty;
    }
}
