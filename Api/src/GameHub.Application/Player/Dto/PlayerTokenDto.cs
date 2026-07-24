namespace GameHub.Player.Dto
{
    /// <summary>Short-lived JWT for a game session.</summary>
    public class PlayerTokenDto
    {
        /// <summary>JWT access token.</summary>
        public string Token { get; set; } = string.Empty;
    }
}
