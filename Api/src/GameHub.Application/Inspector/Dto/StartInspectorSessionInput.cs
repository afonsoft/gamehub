using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Inspector.Dto
{
    public class StartInspectorSessionInput
    {
        [Required]
        public Guid GameId { get; set; }

        public Guid? GameBuildId { get; set; }

        [StringLength(32)]
        public string DevicePreset { get; set; }

        [StringLength(32)]
        public string Resolution { get; set; }
    }
}
