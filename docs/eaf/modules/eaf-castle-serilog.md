# Eaf.Castle.Serilog

## Visão Geral

O módulo `Eaf.Castle.Serilog` fornece integração do Serilog com Castle Windsor, permitindo logging estruturado em aplicações EAF construídas sobre ASP.NET Boilerplate.

## Propósito

Utilizar o Serilog, uma biblioteca de logging estruturado popular e flexível, como o principal framework de logging da aplicação, incluindo a integração com o sistema de logging do Castle Windsor usado pelo ABP.

## Instalação

```bash
Install-Package Serilog.AspNetCore
Install-Package Serilog.Sinks.Console
Install-Package Serilog.Sinks.File
Install-Package Serilog.Enrichers.Environment
Install-Package Serilog.Enrichers.Thread
```

## Configuração

### 1. Configuração em Program.cs

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .WriteTo.File("Logs/eaf-app-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7);
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
```

### 2. Configuração em appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/eaf-app-from-config-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "ThreadId"],
    "Properties": {
      "Application": "EAF Application"
    }
  }
}
```

## Uso

```csharp
public class MyEafTaskService : ApplicationService
{
    private readonly ILogger<MyEafTaskService> _logger;

    public MyEafTaskService(ILogger<MyEafTaskService> logger)
    {
        _logger = logger;
    }

    public void ProcessTask(Guid taskId, string userId)
    {
        _logger.LogInformation("Iniciando processamento da tarefa {TaskId} para o usuário {UserId}", taskId, userId);
        try
        {
            _logger.LogDebug("Detalhes da tarefa: {@TaskDetails}", new { TaskId = taskId, Status = "Em Progresso" });
            _logger.LogInformation("Tarefa {TaskId} processada com sucesso.", taskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao processar tarefa {TaskId}. Usuário: {UserId}", taskId, userId);
        }
    }
}
```

## Troubleshooting

- **Logs Não Aparecem**: Verifique `MinimumLevel` no Serilog e nos sinks
- **Erros de Sink**: Verifique permissões de escrita para o diretório de logs
- **Performance**: Use logging assíncrono (`Serilog.Sinks.Async`) se necessário
