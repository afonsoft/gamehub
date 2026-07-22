namespace GameHub.Monetization.Dto
{
    /// <summary>
    /// Resultado de um break comercial.
    /// </summary>
    public class CommercialBreakResultDto
    {
        /// <summary>Indica se o break foi concluído.</summary>
        public bool Completed { get; set; }

        /// <summary>Duração simulada/em segundos.</summary>
        public int DurationSeconds { get; set; }
    }
}
