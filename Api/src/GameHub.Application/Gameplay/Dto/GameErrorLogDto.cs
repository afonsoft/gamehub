using System;

namespace GameHub.Gameplay.Dto
{
    /// <summary>
    /// DTO for a captured game error log.
    /// </summary>
    public class GameErrorLogDto
    {
        public Guid Id { get; set; }

        public Guid? SessionId { get; set; }

        public Guid GameId { get; set; }

        public Guid? BuildId { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        public string Source { get; set; }

        public string Severity { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
