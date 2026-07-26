using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Admin.Dto
{
    public class ApproveThumbnailInput
    {
        [Required]
        public Guid GameId { get; set; }

        [Required]
        [StringLength(2048)]
        public string ThumbnailUrl { get; set; }
    }
}
