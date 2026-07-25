using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Playtesting.Dto
{
    public class RequestPlaytestInput
    {
        [Required]
        public Guid GameId { get; set; }

        [StringLength(2000)]
        public string Notes { get; set; }
    }
}
