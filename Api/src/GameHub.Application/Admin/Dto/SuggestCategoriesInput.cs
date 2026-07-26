using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Input for suggesting categories for a game.
    /// </summary>
    public class SuggestCategoriesInput
    {
        [Required]
        public Guid GameId { get; set; }
    }
}
