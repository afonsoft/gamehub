using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace GameHub.Catalog.Dto
{
    public class SearchResultDto : PagedResultDto<GameCardDto>
    {
        public SearchResultDto()
        {
        }

        public SearchResultDto(int totalCount, IReadOnlyList<GameCardDto> items)
        {
            TotalCount = totalCount;
            Items = items;
        }
    }
}
