using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Admin.Dto
{
    public class ApproveForPublishingInput
    {
        [Required]
        public Guid GameId { get; set; }

        public Guid? GameBuildId { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }
    }
}
