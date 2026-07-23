# Worker Template

## Visão Geral

O template Worker fornece um serviço de background usando .NET 10.0, ASP.NET Core Worker, e integração com o framework EAF.

## Tecnologias

- **.NET 10.0**: Framework .NET mais recente
- **C# 14**: Linguagem C# mais recente
- **ASP.NET Core Worker**: Template para serviços de background
- **Hangfire**: Sistema de background jobs
- **ASP.NET Boilerplate**: Framework base

## Estrutura do Projeto

```
Templates/Worker/
├── src/
│   ├── Eaf.ProjectName.Worker/          # Aplicação Worker
│   │   ├── Workers/                     # Background Workers
│   │   ├── Jobs/                        # Hangfire Jobs
│   │   └── Services/                    # Serviços do Worker
│   ├── Eaf.ProjectName.Core/            # Camada de Domínio
│   └── Eaf.ProjectName.EntityFrameworkCore/ # Acesso a dados
└── test/                                 # Projetos de Teste
```

## Configuração

### 1. Restaurar Dependências

```bash
dotnet restore
```

### 2. Configuração de Banco de Dados

Edite `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=EafProjectNameWorker;Trusted_Connection=True",
    "Hangfire": "Server=localhost;Database=EafProjectNameHangfire;Trusted_Connection=True"
  }
}
```

### 3. Executar Migrations

```bash
dotnet ef database update
```

### 4. Executar o Projeto

```bash
dotnet run --project src/Eaf.ProjectName.Worker/Eaf.ProjectName.Worker.csproj
```

## Background Workers

### Worker Base

O template usa `BackgroundService` do ASP.NET Core:

```csharp
public class MyWorker : BackgroundService
{
    private readonly ILogger<MyWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MyWorker(ILogger<MyWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var myService = scope.ServiceProvider.GetRequiredService<IMyService>();
            
            await myService.ProcessAsync();
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

### Hangfire Jobs

O template inclui integração com Hangfire para jobs agendados:

```csharp
public class MyRecurringJob
{
    private readonly ILogger<MyRecurringJob> _logger;
    private readonly IMyService _myService;

    public MyRecurringJob(ILogger<MyRecurringJob> logger, IMyService myService)
    {
        _logger = logger;
        _myService = myService;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Executando job agendado");
        await _myService.ProcessAsync();
    }
}
```

Registro do job no Startup:

```csharp
services.AddHangfire(config =>
{
    config.UseSqlServerStorage(Configuration.GetConnectionString("Hangfire"));
});

services.AddHangfireServer();

// Adicionar job recorrente
RecurringJob.AddOrUpdate<MyRecurringJob>(
    "my-recurring-job",
    job => job.ExecuteAsync(),
    Cron.Hourly);
```

## Módulos EAF

O template inclui os módulos EAF principais:

- **Eaf.Middleware.Core**: Módulo base
- **Eaf.Middleware.Worker**: Módulo worker específico
- **Eaf.Castle.Serilog**: Logging estruturado
- **Eaf.OpenTelemetry**: Observabilidade (opcional)

## Configuração de Serviço

### Windows Service

Para executar como Windows Service:

```bash
dotnet publish -c Release
sc create EafWorker binPath= "C:\Path\To\Eaf.Worker.exe"
sc start EafWorker
```

### Linux Service

Para executar como systemd service no Linux:

Crie arquivo `/etc/systemd/system/eaf-worker.service`:

```ini
[Unit]
Description=EAF Worker Service
After=network.target

[Service]
ExecStart=/usr/bin/dotnet /path/to/Eaf.Worker.dll
Restart=always
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

Habilite o serviço:

```bash
sudo systemctl daemon-reload
sudo systemctl enable eaf-worker
sudo systemctl start eaf-worker
```

## Docker

O template inclui suporte a Docker:

```bash
docker build -t eaf-worker .
docker run -d --name eaf-worker eaf-worker
```

## Monitoramento

O template inclui endpoints de health check:

- `/health`: Health check básico
- `/health/ready`: Readiness check
- `/health/live**: Liveness check

## Testes

O template inclui projetos de teste:

- **Eaf.ProjectName.Worker.Tests**: Testes do worker

Execute os testes:

```bash
dotnet test
```

## Troubleshooting

- **Worker Não Inicia**: Verifique as configurações no appsettings.json
- **Hangfire Jobs Não Executam**: Verifique a connection string do Hangfire
- **Permissões**: Verifique se o usuário tem permissões para executar o serviço
- **Logs**: Verifique os logs do Serilog para detalhes de erros
