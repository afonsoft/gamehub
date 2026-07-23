# Eaf.Middleware.Application

## Visão Geral

O módulo `Eaf.Middleware.Application` fornece a camada de aplicação do middleware EAF, incluindo serviços de aplicação base e DTOs compartilhados.

## Propósito

Fornecer a camada de aplicação para o middleware EAF, incluindo serviços base, DTOs e validadores compartilhados entre os módulos.

## Funcionalidades

- **Serviços de Aplicação Base**: Classes base para Application Services
- **DTOs Compartilhados**: DTOs reutilizáveis entre módulos
- **Validadores**: Validadores customizados para DTOs
- **Mapeamento**: Configuração de mapeamento de objetos

## Uso

```csharp
[DependsOn(typeof(EafMiddlewareApplicationModule))]
public class MyModule : AbpModule
{
    public override void PreInitialize()
    {
        // Serviços de aplicação do middleware
        IocManager.Register<IMyService, MyService>(DependencyLifeStyle.Transient);
    }
}
```

## Configuração

```json
{
  "EafMiddleware": {
    "Application": {
      "Enabled": true
    }
  }
}
```
