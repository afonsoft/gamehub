using System;
using System.Collections.Generic;

namespace GameHub.Developer.Dto
{
    public class DeveloperTeamDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PrimaryContactEmail { get; set; }
        public string Country { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<DeveloperTeamMemberDto> Members { get; set; } = new List<DeveloperTeamMemberDto>();
    }
}
