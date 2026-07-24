using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Inspector.Dto
{
    public class LogSdkEventInput
    {
        [Required]
        public Guid SessionId { get; set; }

        [Required]
        [StringLength(64)]
        public string EventType { get; set; }

        [StringLength(2000)]
        public string Payload { get; set; }

        [Required]
        public long SequenceNumber { get; set; }
    }
}
