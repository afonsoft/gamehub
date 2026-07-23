using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using GameHub.Catalog.Dto;
using GameHub.Web.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class RedisGameCatalogCache_Tests
    {
        [Fact]
        public async Task Dado_HomeCacheVazio_Quando_SetarHome_Entao_RetornaDto()
        {
            // Arrange
            var distributedCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            var cache = new RedisGameCatalogCache(distributedCache, NullAbpSession.Instance);
            var dto = new HomeResponseDto
            {
                Highlights = new List<GameCardDto> { new GameCardDto { Title = "Test Game" } }
            };

            // Act
            await cache.SetHomeAsync(dto, TimeSpan.FromMinutes(5));
            var result = await cache.GetHomeAsync();

            // Assert
            result.ShouldNotBeNull();
            result.Highlights.Count.ShouldBe(1);
            result.Highlights[0].Title.ShouldBe("Test Game");
        }

        [Fact]
        public async Task Dado_HomeCachePopulado_Quando_Invalidar_Entao_RetornaNulo()
        {
            // Arrange
            var distributedCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            var cache = new RedisGameCatalogCache(distributedCache, NullAbpSession.Instance);
            await cache.SetHomeAsync(new HomeResponseDto(), TimeSpan.FromMinutes(5));

            // Act
            await cache.InvalidateHomeAsync();
            var result = await cache.GetHomeAsync();

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task Dado_DetalheCacheVazio_Quando_Setar_Entao_RetornaDto()
        {
            var distributedCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            var cache = new RedisGameCatalogCache(distributedCache, NullAbpSession.Instance);
            var detail = new GameDetailDto { Title = "Redis Detail" };

            await cache.SetBySlugAsync("redis-game", detail, TimeSpan.FromMinutes(10));
            var result = await cache.GetBySlugAsync("redis-game");

            result.ShouldNotBeNull();
            result.Title.ShouldBe("Redis Detail");
        }

        [Fact]
        public async Task Dado_BuscaCacheVazio_Quando_Setar_Entao_RetornaResultado()
        {
            var distributedCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            var cache = new RedisGameCatalogCache(distributedCache, NullAbpSession.Instance);
            var search = new SearchResultDto
            {
                TotalCount = 1,
                Items = new List<GameCardDto> { new GameCardDto { Title = "Found" } }
            };

            await cache.SetSearchAsync("key", search, TimeSpan.FromMinutes(2));
            var result = await cache.GetSearchAsync("key");

            result.ShouldNotBeNull();
            result.TotalCount.ShouldBe(1);
            result.Items[0].Title.ShouldBe("Found");
        }

        [Fact]
        public async Task Dado_CategoriasCacheVazio_Quando_Setar_Entao_RetornaLista()
        {
            var distributedCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            var cache = new RedisGameCatalogCache(distributedCache, NullAbpSession.Instance);
            var categories = new ListResultDto<CategoryDto>(new List<CategoryDto>
            {
                new CategoryDto { Id = Guid.NewGuid(), Name = "Arcade", Slug = "arcade" }
            });

            await cache.SetCategoriesAsync(categories, TimeSpan.FromMinutes(30));
            var result = await cache.GetCategoriesAsync();

            result.Items.Count.ShouldBe(1);
            result.Items[0].Name.ShouldBe("Arcade");
        }
    }
}
