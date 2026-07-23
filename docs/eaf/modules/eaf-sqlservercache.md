# Eaf.SqlServerCache

## Visão Geral

O módulo `Eaf.SqlServerCache` fornece implementação de cache distribuído usando SQL Server como backend.

## Propósito

Implementar cache distribuído usando SQL Server, permitindo que múltiplas instâncias da aplicação compartilhem cache através de um banco de dados SQL Server.

## Instalação

```bash
Install-Package Microsoft.Extensions.Caching.SqlServer
```

## Configuração

### 1. Configuração em appsettings.json

```json
{
  "SqlServerCache": {
    "ConnectionString": "Data Source=seu-sqlserver;Initial Catalog=EafCache;Integrated Security=True",
    "SchemaName": "dbo",
    "TableName": "CacheTable",
    "SlidingExpirationInMinutes": 30
  }
}
```

### 2. Configuração em Startup.cs

```csharp
services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = Configuration["SqlServerCache:ConnectionString"];
    options.SchemaName = Configuration["SqlServerCache:SchemaName"];
    options.TableName = Configuration["SqlServerCache:TableName"];
    options.DefaultSlidingExpiration = TimeSpan.FromMinutes(
        int.Parse(Configuration["SqlServerCache:SlidingExpirationInMinutes"])
    );
});
```

### 3. Criação da Tabela de Cache

```sql
CREATE TABLE [dbo].[CacheTable] (
    [Id] [nvarchar](449) NOT NULL,
    [Value] [varbinary](max) NOT NULL,
    [ExpiresAtTime] [datetimeoffset](7) NOT NULL,
    [SlidingExpirationInSeconds] [bigint] NULL,
    [AbsoluteExpiration] [datetimeoffset](7) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);
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

- **Performance**: Cache em SQL Server pode ser mais lento que Redis para alta carga
- **Tamanho do Cache**: Monitore o tamanho da tabela de cache
- **Conexão**: Verifique se a connection string está correta
