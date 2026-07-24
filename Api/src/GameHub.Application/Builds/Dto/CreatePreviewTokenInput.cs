using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Builds.Dto
{
    public class CreatePreviewTokenInput
    {
        [Required]
        public Guid GameId { get; set; }

        [Required]
        [StringLength(64)]
        public string Version { get; set; }
    }
}
