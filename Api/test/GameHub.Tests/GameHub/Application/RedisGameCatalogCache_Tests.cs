using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    }
}
