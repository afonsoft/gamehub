namespace GameHub.Monetization.Dto
{
    /// <summary>
    /// Resultado de um break comercial.
    /// </summary>
    public class CommercialBreakResultDto
    {
        /// <summary>Indica se o break foi concluído.</summary>
        public bool Completed { get; set; }

        /// <summary>Indica se o anúncio foi bloqueado.</summary>
        public bool AdBlocked { get; set; }

        /// <summary>Duração simulada/em segundos.</summary>
        public int DurationSeconds { get; set; }

        /// <summary>Mensagem de erro, se houver.</summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
