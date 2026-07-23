using System;
using Abp.Application.Services.Dto;

namespace GameHub.Gameplay.Dto
{
    /// <summary>Input for retrieving a cloud save.</summary>
    public class GetCloudSaveInput
    {
        /// <summary>Game identifier.</summary>
        public Guid GameId { get; set; }

        /// <summary>Anonymous device id used as fallback when not logged in.</summary>
        public string DeviceId { get; set; }
    }
}
