using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Admin.Dto
{
    public class RequestChangesInput
    {
        [Required]
        public Guid GameId { get; set; }

        [Required]
        [StringLength(4000)]
        public string Reason { get; set; }
    }
}
