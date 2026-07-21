using System;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Feature toggle para o painel administrativo.
    /// </summary>
    public class FeatureFlagDto
    {
        /// <summary>Identificador único.</summary>
        public Guid Id { get; set; }

        /// <summary>Nome técnico da flag.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Descrição do comportamento controlado.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Indica se a flag está ativa.</summary>
        public bool IsEnabled { get; set; }
    }
}
