using System;
using Abp.Application.Services.Dto;

namespace GameHub.Inspector.Dto
{
    public class InspectorWarningDto : EntityDto<Guid>
    {
        public Guid SessionId { get; set; }
        public string Category { get; set; }
        public string Message { get; set; }
        public string Severity { get; set; }
    }
}
