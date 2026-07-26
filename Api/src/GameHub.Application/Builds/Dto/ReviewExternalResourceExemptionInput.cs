using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Builds.Dto
{
    public class ReviewExternalResourceExemptionInput
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        [StringLength(1000)]
        public string ModeratorNotes { get; set; }
    }
}
