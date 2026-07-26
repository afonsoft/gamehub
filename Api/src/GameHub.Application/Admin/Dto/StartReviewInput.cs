using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Admin.Dto
{
    public class StartReviewInput
    {
        [Required]
        public Guid GameId { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }
    }
}
