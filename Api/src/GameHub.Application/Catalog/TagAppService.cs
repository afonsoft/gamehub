using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using GameHub.Admin.Dto;
using GameHub.Authorization;
using GameHub.Catalog.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Catalog
{
    public class TagAppService : GameHubAppServiceBase, ITagAppService
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

        private readonly IRepository<Tag, Guid> _tagRepository;
        private readonly IGameCatalogCache _catalogCache;

        public TagAppService(IRepository<Tag, Guid> tagRepository, IGameCatalogCache catalogCache)
        {
            _tagRepository = tagRepository;
            _catalogCache = catalogCache;
        }

        public async Task<ListResultDto<TagDto>> GetAllAsync()
        {
            var cached = await _catalogCache.GetTagsAsync();
            if (cached != null)
            {
                return cached;
            }

            var tags = await _tagRepository.GetAll()
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.Name)
                .ToListAsync();

            var result = new ListResultDto<TagDto>(ObjectMapper.Map<List<TagDto>>(tags));
            await _catalogCache.SetTagsAsync(result, CacheTtl);
            return result;
        }

        public async Task<TagDto> GetAsync(Guid id)
        {
            var tag = await _tagRepository.GetAsync(id);
            return ObjectMapper.Map<TagDto>(tag);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Tags_Manage)]
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

            await CurrentUnitOfWork.SaveChangesAsync();
            await _catalogCache.InvalidateTagsAsync();
            await _catalogCache.InvalidateHomeAsync();

            return ObjectMapper.Map<TagDto>(tag);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Tags_Manage)]
        public async Task DeleteAsync(Guid id)
        {
            await _tagRepository.DeleteAsync(id);
            await _catalogCache.InvalidateTagsAsync();
            await _catalogCache.InvalidateHomeAsync();
        }
    }
}
