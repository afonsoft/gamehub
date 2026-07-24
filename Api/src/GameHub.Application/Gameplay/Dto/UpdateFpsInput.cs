using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Gameplay.Dto
{
    public class UpdateFpsInput
    {
        [Required]
        public Guid SessionId { get; set; }

        [Required]
        [Range(0, 1000)]
        public double Average { get; set; }

        [Required]
        [Range(0, 1000)]
        public double Min { get; set; }
    }
}
