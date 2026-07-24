using System;
using System.Collections.Generic;

namespace GameHub.Inspector.Dto
{
    public class InspectorSessionDetailDto
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public Guid? GameBuildId { get; set; }
        public DateTime StartedAt { get; set; }
        public string DevicePreset { get; set; }
        public string Resolution { get; set; }
        public string Status { get; set; }
        public List<InspectorSdkEventDto> Events { get; set; } = new List<InspectorSdkEventDto>();
        public List<InspectorWarningDto> Warnings { get; set; } = new List<InspectorWarningDto>();
        public List<InspectorChecklistAnswerDto> ChecklistAnswers { get; set; } = new List<InspectorChecklistAnswerDto>();
    }
}
