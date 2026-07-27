using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Moderation.Dto
{
    public class SubmitUserContentInput
    {
        [Required]
        public Guid GameId { get; set; }

        [Required]
        public UserContentType ContentType { get; set; }

        [Required]
        [StringLength(2000)]
        public string Text { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        /// <summary>Identificador de idempotência enviado pelo cliente.</summary>
        [StringLength(64)]
        public string ClientRequestId { get; set; } = string.Empty;
    }
}
