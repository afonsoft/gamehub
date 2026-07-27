using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Moderation.Dto
{
    public class ModerateUserContentInput
    {
        [Required]
        public Guid ContentId { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>Identificador de idempotência enviado pelo cliente.</summary>
        [StringLength(64)]
        public string ClientRequestId { get; set; } = string.Empty;
    }
}
