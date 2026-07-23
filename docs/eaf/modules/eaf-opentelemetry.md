# Eaf.OpenTelemetry

## Visão Geral

O módulo `Eaf.OpenTelemetry` fornece integração com OpenTelemetry para observabilidade em aplicações EAF.

## Propósito

Coletar telemetria distribuída (traces, métricas e logs) de aplicações EAF usando o padrão OpenTelemetry, permitindo monitoramento e análise de performance.

## Instalação

```bash
Install-Package OpenTelemetry
Install-Package OpenTelemetry.Extensions.Hosting
Install-Package OpenTelemetry.Instrumentation.AspNetCore
Install-Package OpenTelemetry.Instrumentation.EntityFrameworkCore
Install-Package OpenTelemetry.Instrumentation.Http
```

## Configuração

### 1. Configuração em appsettings.json

```json
{
  "OpenTelemetry": {
    "ServiceName": "EAF-Application",
    "ServiceVersion": "1.0.0",
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

### 2. Configuração em Program.cs

```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
    {
        resource.AddService(
            builder.Configuration["OpenTelemetry:ServiceName"],
            builder.Configuration["OpenTelemetry:ServiceVersion"]
        );
    })
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddSource("EAF.*")
            .AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(builder.Configuration["OpenTelemetry:OtlpEndpoint"]);
            });
    })
    .WithMetrics(meterProviderBuilder =>
    {
        meterProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(builder.Configuration["OpenTelemetry:OtlpEndpoint"]);
            });
    });
```

## Uso

```csharp
using System.Diagnostics;

public class MyService : ApplicationService
{
    private readonly ActivitySource _activitySource;

    public MyService()
    {
        _activitySource = new ActivitySource("EAF.MyService");
    }

    public async Task ProcessDataAsync()
    {
        using var activity = _activitySource.StartActivity("ProcessData");
        activity?.SetTag("operation.name", "process_data");

        // Lógica de processamento
        await Task.Delay(100);

        activity?.SetTag("operation.status", "success");
    }
}
```

## Recursos

- **Distributed Tracing**: Rastreamento de requisições através de serviços distribuídos
- **Metrics**: Coleta de métricas de performance e uso
- **Integration**: Integração com backends como Jaeger, Zipkin, Prometheus

## Troubleshooting

- **Endpoint OTLP**: Verifique se o endpoint OTLP está acessível
- **Sampling**: Ajuste a configuração de sampling se necessário
- **Performance**: Verifique o impacto da instrumentação na performance da aplicação
