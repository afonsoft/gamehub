using System.ComponentModel.DataAnnotations;

namespace GameHub.Developer.Dto
{
    public class CreateOrUpdateDeveloperTeamInput
    {
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [Required]
        [StringLength(256)]
        public string PrimaryContactEmail { get; set; }

        [StringLength(128)]
        public string Country { get; set; }
    }
}
