using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Inspector.Dto
{
    public class AddInspectorWarningInput
    {
        [Required]
        public Guid SessionId { get; set; }

        [Required]
        [StringLength(64)]
        public string Category { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        [StringLength(32)]
        public string Severity { get; set; } = "Warning";
    }
}
