using System.ComponentModel.DataAnnotations;

namespace GameHub.Player.Dto
{
    /// <summary>
    /// Input for retrieving recent games.
    /// </summary>
    public class GetRecentInput
    {
        [Range(1, 100)]
        public int Max { get; set; } = 20;
    }
}
