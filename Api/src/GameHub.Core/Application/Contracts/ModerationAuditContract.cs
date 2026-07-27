using System;
using System.Threading.Tasks;

namespace GameHub.Contracts
{
    /// <summary>
    /// Contrato para registros de auditoria de moderação compartilhados entre serviços.
    /// </summary>
    public class ModerationAuditContract
    {
        /// <summary>
        /// Identificador único do registro de auditoria.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tipo da ação de moderação (por exemplo, <c>approve</c>, <c>reject</c> ou <c>ban</c>).
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Identificador do moderador que executou a ação.
        /// </summary>
        public long? ModeratorUserId { get; set; }

        /// <summary>
        /// Tenant do moderador.
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Identificador do recurso moderado.
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Tipo do recurso moderado (por exemplo, <c>user_content</c> ou <c>report</c>).
        /// </summary>
        public string ResourceType { get; set; }

        /// <summary>
        /// Motivo ou observação fornecida para a ação.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Data e hora UTC em que a ação foi realizada.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Escreve registros de auditoria de moderação para compliance e forense cross-cutting.
    /// </summary>
    public interface IModerationAuditWriter
    {
        /// <summary>
        /// Grava um registro de auditoria de forma assíncrona.
        /// </summary>
        /// <param name="entry">Registro operacional da ação.</param>
        /// <returns>Uma tarefa que representa a gravação.</returns>
        Task WriteAsync(ModerationAuditContract entry);
    }
}
