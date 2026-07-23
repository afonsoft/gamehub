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
    public class CategoryAppService : GameHubAppServiceBase, ICategoryAppService
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IGameCatalogCache _catalogCache;

        public CategoryAppService(IRepository<Category, Guid> categoryRepository, IGameCatalogCache catalogCache)
        {
            _categoryRepository = categoryRepository;
            _catalogCache = catalogCache;
        }

        public async Task<ListResultDto<CategoryDto>> GetAllAsync()
        {
            var cached = await _catalogCache.GetCategoriesAsync();
            if (cached != null)
            {
                return cached;
            }

            var categories = await _categoryRepository.GetAll()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            var result = new ListResultDto<CategoryDto>(ObjectMapper.Map<List<CategoryDto>>(categories));
            await _catalogCache.SetCategoriesAsync(result, CacheTtl);
            return result;
        }

        public async Task<CategoryDto> GetAsync(Guid id)
        {
            var category = await _categoryRepository.GetAsync(id);
            return ObjectMapper.Map<CategoryDto>(category);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Categories_Manage)]
        public async Task<CategoryDto> CreateOrUpdateAsync(CreateOrUpdateCategoryInput input)
        {
            Category category;
            if (input.Id.HasValue)
            {
                category = await _categoryRepository.GetAsync(input.Id.Value);
                category.Name = input.Name;
                category.Slug = input.Slug ?? input.Name.ToLowerInvariant().Replace(" ", "-");
                category.SortOrder = input.SortOrder;
                category.IsActive = input.IsActive;
            }
            else
            {
                category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = input.Name,
                    Slug = input.Slug ?? input.Name.ToLowerInvariant().Replace(" ", "-"),
                    SortOrder = input.SortOrder,
                    IsActive = input.IsActive
                };

                await _categoryRepository.InsertAsync(category);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            await _catalogCache.InvalidateCategoriesAsync();
            await _catalogCache.InvalidateHomeAsync();

            return ObjectMapper.Map<CategoryDto>(category);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Categories_Manage)]
        public async Task DeleteAsync(Guid id)
        {
            await _categoryRepository.DeleteAsync(id);
            await _catalogCache.InvalidateCategoriesAsync();
            await _catalogCache.InvalidateHomeAsync();
        }
    }
}
