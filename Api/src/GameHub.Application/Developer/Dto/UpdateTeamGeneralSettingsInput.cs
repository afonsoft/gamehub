using System.ComponentModel.DataAnnotations;

namespace GameHub.Developer.Dto
{
    /// <summary>
    /// Input for updating a developer team's general settings.
    /// </summary>
    public class UpdateTeamGeneralSettingsInput
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
