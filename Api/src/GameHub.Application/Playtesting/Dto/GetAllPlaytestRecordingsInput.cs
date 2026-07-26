using System;
using Abp.Application.Services.Dto;

namespace GameHub.Playtesting.Dto
{
    public class GetAllPlaytestRecordingsInput : PagedAndSortedResultRequestDto
    {
        public Guid? GameId { get; set; }

        public string DeviceType { get; set; }
    }
}
