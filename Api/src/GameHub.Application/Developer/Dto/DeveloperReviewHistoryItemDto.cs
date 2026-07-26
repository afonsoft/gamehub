using System;

namespace GameHub.Developer.Dto
{
    /// <summary>
    /// Histórico de uma decisão de revisão do desenvolvedor.
    /// </summary>
    public class DeveloperReviewHistoryItemDto
    {
        /// <summary>Identificador da revisão.</summary>
        public Guid Id { get; set; }

        /// <summary>Identificador do jogo.</summary>
        public Guid GameId { get; set; }

        /// <summary>Identificador do build associado.</summary>
        public Guid? GameBuildId { get; set; }

        /// <summary>Status atual da revisão.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Decisão do moderador, quando disponível.</summary>
        public string Decision { get; set; } = string.Empty;

        /// <summary>Observações da revisão.</summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>Data UTC de criação.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Data UTC de conclusão, quando disponível.</summary>
        public DateTime? CompletedAt { get; set; }
    }
}
