using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using GameHub.Admin.Dto;
using GameHub.Catalog.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Catalog
{
    public class TagAppService : GameHubAppServiceBase, ITagAppService
    {
        private readonly IRepository<Tag, Guid> _tagRepository;

        public TagAppService(IRepository<Tag, Guid> tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<ListResultDto<TagDto>> GetAllAsync()
        {
            var tags = await _tagRepository.GetAll()
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.Name)
                .ToListAsync();

            return new ListResultDto<TagDto>(ObjectMapper.Map<List<TagDto>>(tags));
        }

        public async Task<TagDto> GetAsync(Guid id)
        {
            var tag = await _tagRepository.GetAsync(id);
            return ObjectMapper.Map<TagDto>(tag);
        }

        public async Task<TagDto> CreateOrUpdateAsync(CreateOrUpdateTagInput input)
        {
            Tag tag;
            if (input.Id.HasValue)
            {
                tag = await _tagRepository.GetAsync(input.Id.Value);
                tag.Name = input.Name;
                tag.Slug = input.Slug ?? input.Name.ToLowerInvariant().Replace(" ", "-");
            }
            else
            {
                tag = new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = input.Name,
                    Slug = input.Slug ?? input.Name.ToLowerInvariant().Replace(" ", "-"),
                };

                await _tagRepository.InsertAsync(tag);
            }

            return ObjectMapper.Map<TagDto>(tag);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _tagRepository.DeleteAsync(id);
        }
    }
}
