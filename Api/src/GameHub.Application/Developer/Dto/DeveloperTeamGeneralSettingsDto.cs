using System;

namespace GameHub.Developer.Dto
{
    /// <summary>
    /// General settings of a developer team.
    /// </summary>
    public class DeveloperTeamGeneralSettingsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PrimaryContactEmail { get; set; }
        public string Country { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
