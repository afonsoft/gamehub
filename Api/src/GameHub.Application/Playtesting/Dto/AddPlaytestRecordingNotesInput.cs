using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Playtesting.Dto
{
    /// <summary>
    /// Input for adding notes to a playtest recording.
    /// </summary>
    public class AddPlaytestRecordingNotesInput
    {
        [Required]
        public Guid RecordingId { get; set; }

        [StringLength(4000)]
        public string Notes { get; set; }
    }
}
