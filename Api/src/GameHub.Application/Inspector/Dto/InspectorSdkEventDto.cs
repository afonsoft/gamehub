using System;
using Abp.Application.Services.Dto;

namespace GameHub.Inspector.Dto
{
    public class InspectorSdkEventDto : EntityDto<Guid>
    {
        public Guid SessionId { get; set; }
        public string EventType { get; set; }
        public string Payload { get; set; }
        public long SequenceNumber { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
