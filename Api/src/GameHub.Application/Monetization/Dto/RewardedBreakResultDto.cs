namespace GameHub.Monetization.Dto
{
    /// <summary>
    /// Resultado de um rewarded ad break.
    /// </summary>
    public class RewardedBreakResultDto
    {
        /// <summary>Indica se o usuário completou o rewarded ad.</summary>
        public bool Completed { get; set; }

        /// <summary>Indica se a recompensa foi concedida.</summary>
        public bool RewardGranted { get; set; }

        /// <summary>Indica se o anúncio foi bloqueado.</summary>
        public bool AdBlocked { get; set; }

        /// <summary>Mensagem de erro, se houver.</summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
