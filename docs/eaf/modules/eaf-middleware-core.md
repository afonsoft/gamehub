# Eaf.Middleware.Core

## Visão Geral

O módulo `Eaf.Middleware.Core` é o módulo base do middleware EAF, fornecendo funcionalidades core compartilhadas entre os outros módulos de middleware.

## Propósito

Fornecer a base para os módulos de middleware EAF, incluindo configurações compartilhadas, serviços base e funcionalidades comuns.

## Funcionalidades

- **Configuração Centralizada**: Configurações compartilhadas entre módulos
- **Serviços Base**: Classes base para serviços de aplicação
- **Validação**: Validadores customizados compartilhados
- **Extensões**: Métodos de extensão para funcionalidades comuns

## Uso

Este módulo é automaticamente incluído quando você usa qualquer outro módulo de middleware EAF.

```csharp
[DependsOn(typeof(EafMiddlewareCoreModule))]
public class MyModule : AbpModule
{
    public override void PreInitialize()
    {
        // Acessa configurações centralizadas
        var configuration = Configuration;
    }
}
```

## Configuração

```json
{
  "EafMiddleware": {
    "Core": {
      "Enabled": true,
      "Version": "10.0.0"
    }
  }
}
```
