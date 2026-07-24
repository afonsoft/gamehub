using System.ComponentModel.DataAnnotations;

namespace GameHub.Player.Dto
{
    public class SetLanguageInput
    {
        [Required]
        [StringLength(16)]
        public string Language { get; set; } = "en-US";
    }
}
