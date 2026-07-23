using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using GameHub.Catalog;
using GameHub.Catalog.Dto;
using Microsoft.Extensions.Caching.Memory;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class InMemoryGameCatalogCache_Tests
    {
        [Fact]
        public async Task Dado_CacheVazio_Quando_SetarDetalhe_Entao_RetornaDetalhe()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cache = new InMemoryGameCatalogCache(memoryCache, NullAbpSession.Instance);
            var detail = new GameDetailDto { Title = "Cached Detail" };

            await cache.SetBySlugAsync("cached-game", detail, TimeSpan.FromMinutes(10));
            var result = await cache.GetBySlugAsync("cached-game");

            result.ShouldNotBeNull();
            result.Title.ShouldBe("Cached Detail");
        }

        [Fact]
        public async Task Dado_CachePopulado_Quando_Invalidar_Entao_RetornaNulo()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cache = new InMemoryGameCatalogCache(memoryCache, NullAbpSession.Instance);

            await cache.SetBySlugAsync("game", new GameDetailDto(), TimeSpan.FromMinutes(10));
            await cache.InvalidateBySlugAsync("game");

            var result = await cache.GetBySlugAsync("game");
            result.ShouldBeNull();
        }

        [Fact]
        public async Task Dado_Categorias_Quando_Setar_Entao_RetornaLista()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cache = new InMemoryGameCatalogCache(memoryCache, NullAbpSession.Instance);
            var categories = new ListResultDto<CategoryDto>(new List<CategoryDto>
            {
                new CategoryDto { Id = Guid.NewGuid(), Name = "Action", Slug = "action" }
            });

            await cache.SetCategoriesAsync(categories, TimeSpan.FromMinutes(30));
            var result = await cache.GetCategoriesAsync();

            result.Items.Count.ShouldBe(1);
            result.Items[0].Name.ShouldBe("Action");
        }

        [Fact]
        public async Task Dado_Tags_Quando_Setar_Entao_RetornaLista()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cache = new InMemoryGameCatalogCache(memoryCache, NullAbpSession.Instance);
            var tags = new ListResultDto<TagDto>(new List<TagDto>
            {
                new TagDto { Id = Guid.NewGuid(), Name = "2D", Slug = "2d" }
            });

            await cache.SetTagsAsync(tags, TimeSpan.FromMinutes(30));
            var result = await cache.GetTagsAsync();

            result.Items.Count.ShouldBe(1);
            result.Items[0].Name.ShouldBe("2D");
        }
    }
}
