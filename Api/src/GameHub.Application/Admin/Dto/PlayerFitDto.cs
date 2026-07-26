using System;
using System.Collections.Generic;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Player fit metrics: retention and stickiness for a game.
    /// </summary>
    public class PlayerFitDto
    {
        public Guid GameId { get; set; }

        public string GameTitle { get; set; } = string.Empty;

        public double Retention1d { get; set; }

        public double Retention7d { get; set; }

        public double Retention30d { get; set; }

        public double Stickiness { get; set; }

        public double CategoryAverageStickiness { get; set; }

        public string Benchmark { get; set; } = string.Empty;

        public List<string> Suggestions { get; set; } = new();
    }
}
