# https://aspnetboilerplate.com/Pages/Documents/PerRequestRedisCache

## PerRequestRedisCache

|     |     |     |
| --- | --- | --- |
| |     |     |
| --- | --- |
|  | × | | search |  |

Custom Search

|     |     |
| --- | --- |
|  | Sort by<br>Relevance<br>Date |

Version

latest (v10.4)v10.3v10.2v10.1v10.0v9.4.2v9.4.1v9.4.0v9.3.0v9.2.0v9.1.3v9.1v9.0v8.4v8.3v8.2v8.1v8.0v7.4v7.3v7.2v7.1v7.0-rc1v7.0v6.6.1v6.6.0v6.5.0v6.4-rc1v6.4.0v6.3.1v6.3v6.2v6.1.1v6.1.0v6.0v5.14v5.13v5.12v5.10.1v5.10v5.9v5.8v5.7v5.6v5.5v5.4v5.3v5.2v5.1.0v5.0.0v4.21v4.20v4.19v4.18v4.17v4.16v4.15v4.14v4.13v4.12v4.11.0v4.10.1v4.10.0v4.9.0v4.8.1v4.8.0v4.7.0v4.6.0v4.5.0v4.4.0v4.3.0v4.2.0v4.1.0v4.0.2v4.0.1v4.0.0v3.9.0v3.8.3v3.8.2v3.8.1v3.8.0v3.7.2v3.7.1v3.7.0v3.6.2v3.6.1v3.6.0v3.5.0v3.4.0v3.3.0v3.2.5v3.2.4v3.2.3v3.2.2v3.2.1v3.2.0v3.1.2v3.1.1v3.1.0v3.0.0-beta3v3.0.0-rc2v3.0.0-beta2v3.0.0-rc1v3.0.0-beta1v3.0.0v2.3.0v2.2.2v2.2.1v2.2.0v2.1.3v2.1.2v2.1.1v2.1.0-beta4v2.1.0-beta3v2.1.0-beta2v2.1.0-beta1v2.1.0v2.0.2v2.0.1v2.0.0-preview4v2.0.0-rc3v2.0.0-preview3v2.0.0-rc2v2.0.0-preview1v2.0.0v2.0.0-rcv1.5.2v1.5.1v1.5.0v1.4.3v1.4.2v1.4.1v1.4.0.0v1.3.1.0v1.3.0.0v1.2.2.0v1.2.1.0v1.2.0.0v1.1.3.0v1.1.1.0v1.1.0.0v1.0.0.0v0.10.3.2Menu

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/PerRequestRedisCache.md)

In this document

### Per Request Redis Cache [Anchor](https://aspnetboilerplate.com/Pages/Documents/PerRequestRedisCache\#per-request-redis-cache)

When you use a cache object derived from the default Redis cache implementation (`ICacheManager` with Redis), every action you make is executed on Redis. Although Redis works much faster than many operations such as reading from the database, making an API query, Redis queries have a performance cost (this cost may vary depending on the location of your Redis server to your server). If your project uses too many Redis queries, this can sometimes cause performance issues and also cause a bottleneck. One method that can solve this is to cache these Redis queries locally for some non-critical data you query from Redis many times in your project.

For example, let's say you are caching about localization, and you keep the dictionary of these localization keys in Redis. You may need to go to Redis dozens of times and read the value of the relevant localization key from Redis for the localization process on each page load. Instead, you can pull that data from the Redis once and make queries on it. You can use `IAbpPerRequestRedisCacheManager` implementation instead of `ICacheManager` (with Redis) for data whose instant change is not critical.

Using `IAbpPerRequestRedisCacheManager` is the same as using `ICacheManager` and returns objects of the same data type. The only difference is that when a cache object is created using `IAbpPerRequestRedisCacheManager`, Redis queries are cached locally on the `HttpContext`. Thus, a query is sent to Redis only once within a single HTTP request. Repeated queries that follow do not go to Redis, and data from previous Redis response is used. In this way, you can improve performance.

### Usage of IAbpPerRequestRedisCacheManager [Anchor](https://aspnetboilerplate.com/Pages/Documents/PerRequestRedisCache\#usage-of-iabpperrequestrediscachemanager)

- Import `Abp.AspNetCore.PerRequestRedisCache` NuGet package.

- Add depends on `AbpAspNetCorePerRequestRedisCacheModule`

Copy


```csharp
[DependsOn(typeof(AbpAspNetCorePerRequestRedisCacheModule))]
public class MyModule : AbpModule
{
//...
```

- Then get cache using `IAbpPerRequestRedisCacheManager`

Copy


```csharp
public class TestAppService : ApplicationService
{
      private readonly IAbpPerRequestRedisCacheManager _cacheManager;

      public TestAppService(IAbpPerRequestRedisCacheManager cacheManager)
      {
          _cacheManager = cacheManager;
      }

      private ICache GetMyCache() => _cacheManager.GetCache("MyCache"); // get cache using `IAbpPerRequestRedisCacheManager`
```


It is all you have to do in order to use per request Redis cache implementation. Now you can use all caching features. See caching [documentation](https://aspnetboilerplate.com/Pages/Documents/Caching) for more information.


Note: You must enable Redis to use `IAbpPerRequestRedisCacheManager`. ( [Caching documentation](https://aspnetboilerplate.com/Pages/Documents/Caching#redis-cache-integration))

### Replace ICacheManager with IAbpPerRequestRedisCacheManager [Anchor](https://aspnetboilerplate.com/Pages/Documents/PerRequestRedisCache\#replace-icachemanager-with-iabpperrequestrediscachemanager)

In order to replace current implementation of the cache with `PerRequestRedisCache`, after you import NuGet package and add depends on, enable Redis with `usePerRequestRedisCache: true` parameter

Copy

```csharp
Configuration.Caching.UseRedis(usePerRequestRedisCache: true);
```

This will replace `ICacheManager` with `IAbpPerRequestRedisCacheManager`. Your entire project will start working with `IAbpPerRequestRedisCacheManager`.

[GitHub link](https://github.com/aspnetboilerplate/aspnetboilerplate/tree/dev/src/Abp.AspNetCore.PerRequestRedisCache)

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe