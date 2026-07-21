using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Moderation.Dto
{
    /// <summary>
    /// Dados para submeter um report de conteúdo de jogo.
    /// </summary>
    public class UserReportInput
    {
        /// <summary>Identificador do jogo reportado.</summary>
        [Required]
        public Guid GameId { get; set; }

        /// <summary>Motivo do report.</summary>
        [Required]
        [StringLength(128)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>Descrição detalhada opcional.</summary>
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;
    }
}
