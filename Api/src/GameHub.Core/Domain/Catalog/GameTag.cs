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
    public class GameTag
    {
        [Required]
        public Guid GameId { get; set; }

        [Required]
        public Guid TagId { get; set; }

        public virtual Game Game { get; set; }

        public virtual Tag Tag { get; set; }
    }
}
