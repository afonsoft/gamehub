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
        [StringLength(4000)]
        public string Text { get; set; }
    }
}
