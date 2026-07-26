# Eaf.Middleware.Web.Core

## Visão Geral

O módulo `Eaf.Middleware.Web.Core` fornece funcionalidades web core do middleware EAF, incluindo middleware HTTP e configurações web compartilhadas.

## Propósito

Fornecer a camada web core para o middleware EAF, incluindo middleware HTTP, filtros e configurações web compartilhadas entre os módulos web.

## Funcionalidades

- **Middleware HTTP**: Middleware HTTP customizados compartilhados
- **Filtros**: Filtros MVC e Web API compartilhados
- **Configurações Web**: Configurações web centralizadas
- **CORS**: Configuração de CORS compartilhada
- **Cache**: Configuração centralizada de `ICacheManager` e `IDistributedCache`

## Uso

```csharp
[DependsOn(typeof(EafMiddlewareWebCoreModule))]
public class MyWebModule : AbpModule
{
    public override void PreInitialize()
    {
        // Configurações web do middleware
        Configuration.Modules.AbpAspNetCore().Mvc.Configure(options =>
        {
            // Customizações MVC
        });
    }
}
```

## Configuração

O `MiddlewareWebCoreModule` é a origem da configuração base de cache para
aplicações EAF/ABP:

- `CacheConfigurer` seleciona o provider do `ICacheManager`, incluindo Redis e
  SQL Server.
- `RedisConfigurer` registra `IDistributedCache` com Redis quando
  `RedisCache:IsEnabled` ou `RedisCache:IsRedisEnabled` está habilitado.
- `RedisCache:ConnectionString` e `RedisCache:DatabaseId` são consumidos pelo
  EAF.

Aplicações consumidoras, como o GameHub, não devem repetir
`Configuration.Caching.UseRedis(...)` no próprio `WebHostModule`. Devem
consumir `ICacheManager`/`IDistributedCache` e registrar apenas caches ou
componentes específicos da aplicação. O backplane Redis do SignalR é uma
configuração separada de transporte.

```json
{
  "EafMiddleware": {
    "WebCore": {
      "Enabled": true,
      "Cors": {
        "AllowedOrigins": ["*"]
      }
    }
  }
}
```
