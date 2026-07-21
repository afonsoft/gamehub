using GameHub.Moderation;
using GameHub.Gameplay;
using GameHub.Developers;
using GameHub.Catalog;
using GameHub.Builds;
using GameHub;
using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Catalog
{
    public class GameCategory
    {
        [Required]
        public Guid GameId { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        public virtual Game Game { get; set; }

        public virtual Category Category { get; set; }
    }
}
