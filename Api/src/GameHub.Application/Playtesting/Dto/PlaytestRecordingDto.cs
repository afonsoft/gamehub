using System;

namespace GameHub.Playtesting.Dto
{
    /// <summary>
    /// Recording captured during a playtest session.
    /// </summary>
    public class PlaytestRecordingDto
    {
        public Guid Id { get; set; }
        public Guid PlaytestSessionId { get; set; }
        public string Url { get; set; }
        public int DurationSeconds { get; set; }
        public string DeviceType { get; set; }
        public string CountryCode { get; set; }
        public string ConsoleOutput { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
