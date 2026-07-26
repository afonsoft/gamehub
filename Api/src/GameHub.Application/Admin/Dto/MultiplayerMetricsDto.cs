using System;

namespace GameHub.Admin.Dto
{
    public class MultiplayerMetricsDto
    {
        public Guid GameId { get; set; }

        public DateTime Date { get; set; }

        public int MatchesCreated { get; set; }

        public int ActiveMatches { get; set; }

        public int PlayersConnected { get; set; }
    }
}
