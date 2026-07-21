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
    public class CategoryAppService : ApplicationService, ICategoryAppService
    {
        private readonly IRepository<Category, Guid> _categoryRepository;

        public CategoryAppService(IRepository<Category, Guid> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<ListResultDto<CategoryDto>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAll()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            return new ListResultDto<CategoryDto>(ObjectMapper.Map<List<CategoryDto>>(categories));
        }

        public async Task<CategoryDto> GetAsync(Guid id)
        {
            var category = await _categoryRepository.GetAsync(id);
            return ObjectMapper.Map<CategoryDto>(category);
        }

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

            return ObjectMapper.Map<CategoryDto>(category);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _categoryRepository.DeleteAsync(id);
        }
    }
}
