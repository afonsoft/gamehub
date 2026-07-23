# Eaf.Middleware.Worker

## Visão Geral

O módulo `Eaf.Middleware.Worker` fornece funcionalidades para background jobs e workers no middleware EAF.

## Propósito

Fornecer suporte para background jobs e workers usando o sistema de background jobs do ABP, integrado com os módulos de middleware EAF.

## Funcionalidades

- **Background Jobs**: Jobs em background do ABP integrados com middleware
- **Hangfire**: Integração com Hangfire para jobs agendados
- **Workers**: Workers de longa duração
- **Fila de Jobs**: Gerenciamento de filas de jobs

## Configuração

```json
{
  "EafMiddleware": {
    "Worker": {
      "Enabled": true,
      "Hangfire": {
        "ConnectionString": "Data Source=seu-sqlserver;Initial Catalog=EafHangfire;Integrated Security=True"
      }
    }
  }
}
```

## Uso

```csharp
[DependsOn(typeof(EafMiddlewareWorkerModule))]
public class MyWorkerModule : AbpModule
{
    public override void PreInitialize()
    {
        // Configuração de background jobs
    }
}
```

## Background Jobs

```csharp
public class MyBackgroundJob : BackgroundJob<MyBackgroundJobArgs>
{
    public override Task ExecuteAsync(MyBackgroundJobArgs args)
    {
        // Lógica do background job
        return Task.CompletedTask;
    }
}
```
