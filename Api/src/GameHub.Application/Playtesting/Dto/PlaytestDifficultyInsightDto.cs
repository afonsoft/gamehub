using System;
using System.Collections.Generic;

namespace GameHub.Playtesting.Dto
{
    /// <summary>
    /// Difficulty balancing insight aggregated from playtest level events.
    /// </summary>
    public class PlaytestDifficultyInsightDto
    {
        public Guid GameId { get; set; }

        public string GameTitle { get; set; } = string.Empty;

        public List<LevelDifficultyDto> Levels { get; set; } = new();
    }

    public class LevelDifficultyDto
    {
        public string Level { get; set; } = string.Empty;

        public long Starts { get; set; }

        public long Deaths { get; set; }

        public long Restarts { get; set; }

        public long Completions { get; set; }

        public double CompletionRate { get; set; }

        public double AvgDeathsPerStart { get; set; }
    }
}
