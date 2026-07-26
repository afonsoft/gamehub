using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Input for validating SEO fields of a game.
    /// </summary>
    public class ValidateSeoInput
    {
        [Required]
        public Guid GameId { get; set; }
    }
}
