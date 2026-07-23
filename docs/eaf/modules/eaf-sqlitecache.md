# Eaf.SqliteCache

## Visão Geral

O módulo `Eaf.SqliteCache` fornece implementação de cache distribuído usando SQLite como backend.

## Propósito

Implementar cache distribuído usando SQLite, ideal para cenários onde você precisa de cache persistente mas não quer depender de SQL Server ou Redis.

## Instalação

```bash
Install-Package Microsoft.Extensions.Caching.Sqlite
```

## Configuração

### 1. Configuração em appsettings.json

```json
{
  "SqliteCache": {
    "ConnectionString": "Data Source=eaf-cache.db",
    "TableName": "CacheTable",
    "SlidingExpirationInMinutes": 30
  }
}
```

### 2. Configuração em Startup.cs

```csharp
services.AddDistributedSqliteCache(options =>
{
    options.ConnectionString = Configuration["SqliteCache:ConnectionString"];
    options.TableName = Configuration["SqliteCache:TableName"];
    options.DefaultSlidingExpiration = TimeSpan.FromMinutes(
        int.Parse(Configuration["SqliteCache:SlidingExpirationInMinutes"])
    );
});
```

## Uso

```csharp
public class MyService : ApplicationService
{
    private readonly IDistributedCache _cache;

    public MyService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<MyData> GetDataAsync(string key)
    {
        var cachedData = await _cache.GetAsync(key);
        if (cachedData != null)
        {
            return JsonSerializer.Deserialize<MyData>(cachedData);
        }

        var data = await FetchDataFromDatabaseAsync(key);
        var serializedData = JsonSerializer.SerializeToUtf8Bytes(data);
        await _cache.SetAsync(key, serializedData, new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(30)
        });
        return data;
    }
}
```

## Troubleshooting

- **Performance**: SQLite pode ter limitações de concorrência em alta carga
- **Arquivo de Lock**: Verifique se o arquivo de cache está acessível e não está bloqueado
- **Concorrência**: Use com cuidado em cenários de alta concorrência
