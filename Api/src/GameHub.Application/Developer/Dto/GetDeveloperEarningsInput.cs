using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Developer.Dto
{
    public class GetDeveloperEarningsInput
    {
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        [Range(1, 100)]
        public int MaxResultCount { get; set; } = 50;
    }
}
