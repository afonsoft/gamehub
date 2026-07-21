using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Uow;
using GameHub.Catalog;
using GameHub.Admin.Dto;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class CategoryAppService_Tests : ProjectNameTestBase
    {
        private readonly ICategoryAppService _categoryAppService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public CategoryAppService_Tests()
        {
            _categoryAppService = LocalIocManager.Resolve<ICategoryAppService>();
            _unitOfWorkManager = LocalIocManager.Resolve<IUnitOfWorkManager>();
        }

        [Fact]
        public async Task Dado_CategoriaCadastrada_Quando_Buscar_Entao_DeveRetornar()
        {
            var createdId = Guid.NewGuid();
            UsingDbContext(context =>
            {
                context.Categories.Add(new Category
                {
                    Id = createdId,
                    TenantId = AbpSession.TenantId,
                    Name = "Action",
                    Slug = "action",
                    SortOrder = 1,
                    IsActive = true
                });
            });

            var fetched = await _categoryAppService.GetAsync(createdId);
            fetched.ShouldNotBeNull();
            fetched.Name.ShouldBe("Action");

            var all = await _categoryAppService.GetAllAsync();
            all.Items.Any(c => c.Id == createdId).ShouldBeTrue();
        }

        [Fact]
        public async Task Dado_CategoriaExistente_Quando_Atualizar_Entao_DeveRefletirAlteracoes()
        {
            var createdId = Guid.NewGuid();
            UsingDbContext(context =>
            {
                context.Categories.Add(new Category
                {
                    Id = createdId,
                    TenantId = AbpSession.TenantId,
                    Name = "Racing",
                    Slug = "racing",
                    SortOrder = 2,
                    IsActive = true
                });
            });

            var updateInput = new CreateOrUpdateCategoryInput
            {
                Id = createdId,
                Name = "Racing Updated",
                Slug = "racing-updated",
                SortOrder = 3
            };

            var updated = await _categoryAppService.CreateOrUpdateAsync(updateInput);

            updated.Name.ShouldBe("Racing Updated");
            updated.Slug.ShouldBe("racing-updated");
        }
    }
}
