namespace GameHub.Player.Dto
{
    /// <summary>Public player profile returned to games via the SDK.</summary>
    public class PlayerProfileDto
    {
        /// <summary>Player username.</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>Avatar URL, if available.</summary>
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
