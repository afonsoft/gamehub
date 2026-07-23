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
