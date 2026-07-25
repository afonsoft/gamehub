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
    }
}
