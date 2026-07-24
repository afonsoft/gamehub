using System.ComponentModel.DataAnnotations;

namespace GameHub.Builds.Dto
{
    public class ValidatePreviewInput
    {
        [Required]
        [StringLength(4096)]
        public string Token { get; set; }
    }
}
