using System;
using Abp.Application.Services.Dto;

namespace GameHub.Inspector.Dto
{
    public class InspectorSessionDto : EntityDto<Guid>
    {
        public Guid GameId { get; set; }
        public Guid? GameBuildId { get; set; }
        public DateTime StartedAt { get; set; }
        public string DevicePreset { get; set; }
        public string Resolution { get; set; }
        public string Status { get; set; }
    }
}
