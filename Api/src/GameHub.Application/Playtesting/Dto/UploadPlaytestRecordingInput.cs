using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Playtesting.Dto
{
    public class UploadPlaytestRecordingInput
    {
        [Required]
        public Guid PlaytestId { get; set; }

        [Required]
        [StringLength(2048)]
        public string RecordingUrl { get; set; }

        public int DurationSeconds { get; set; }

        [StringLength(32)]
        public string DeviceType { get; set; }

        [StringLength(2)]
        public string CountryCode { get; set; }

        [StringLength(4000)]
        public string ConsoleOutput { get; set; }
    }
}
