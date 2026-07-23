# Background Jobs no EAF (Enterprise Application Foundation)

## 1. Introdução aos Background Jobs no EAF

Background jobs são tarefas que são executadas de forma assíncrona, em segundo plano, fora do ciclo de vida de uma solicitação HTTP. Eles são essenciais para aplicações que precisam processar operações demoradas ou tarefas agendadas sem bloquear a interface do usuário.

*   **Offloading de Tarefas Longas**: Executar operações demoradas (e.g., processamento de relatórios, envio de e-mails em massa, integrações com sistemas externos) fora do ciclo de requisição-resposta da web, melhorando a responsividade da UI.
*   **Tarefas Agendadas**: Executar tarefas em horários específicos ou em intervalos regulares (e.g., limpeza de dados noturna, sincronizações periódicas).
*   **Aumento da Robustez**: Desacoplar a execução de tarefas da interação do usuário, permitindo que elas sejam retentadas em caso de falhas temporárias.

O EAF é uma plataforma open source que utiliza primariamente o sistema de background jobs do ASP.NET Boilerplate (ABP). O ABP fornece uma abstração (`IBackgroundJobManager`) para enfileirar jobs e integra-se por padrão com o **Hangfire** (`Abp.HangFire.AspNetCore`) como o provedor de execução e persistência de jobs.

Este documento descreve como criar, enfileirar e gerenciar background jobs no contexto do EAF.

## 2. Criando e Enfileirando Background Jobs

### 2.1. Definindo um Job

Um background job no EAF/ABP é uma classe C# que executa uma tarefa específica. Existem algumas maneiras de definir um job:

*   Herdando de `BackgroundJob<TArgs>`: Esta classe base do ABP fornece um método `Execute(TArgs args)` síncrono. Ela também gerencia o Unit of Work (UoW) por padrão (um UoW é aberto antes de `Execute` e completado depois).
*   Implementando `IBackgroundJob<TArgs>`: Requer a implementação de um método `Execute(TArgs args)`. Permite mais controle sobre o UoW se necessário (pode-se injetar `IUnitOfWorkManager`).
*   Implementando `IAsyncBackgroundJob<TArgs>`: Similar a `IBackgroundJob`, mas com um método `ExecuteAsync(TArgs args)` para operações assíncronas.

Todos os jobs devem ser registrados na DI, geralmente como `ITransientDependency`.

**Exemplo EAF (Job para Processar um Relatório)**:
```csharp
// No projeto Eaf.MyModule.Core ou Eaf.MyModule.Application
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Uow;
using Microsoft.Extensions.Logging; // Usar ILogger do .NET Core
using System;
using System.Threading.Tasks;

// 1. Argumentos do Job (devem ser serializáveis)
public class ReportGenerationArgs
{
    public Guid ReportId { get; set; }
    public long RequestingUserId { get; set; }
    // Outros parâmetros necessários para gerar o relatório
}

// 2. Definição do Job
public class ReportGeneratorJob : AsyncBackgroundJob<ReportGenerationArgs>, ITransientDependency
{
    private readonly ILogger<ReportGeneratorJob> _logger;
    private readonly IReportService _reportService; // Um serviço de domínio/aplicação EAF

    public ReportGeneratorJob(
        ILogger<ReportGeneratorJob> logger,
        IReportService reportService) // Injetar dependências necessárias
    {
        _logger = logger;
        _reportService = reportService;
    }

    [UnitOfWork] // Opcional se já herda de BackgroundJob, mas explícito para AsyncBackgroundJob se não herdar de BackgroundJob.
                 // A classe base BackgroundJob<TArgs> já é UnitOfWork por padrão.
                 // Para AsyncBackgroundJob<TArgs>, se não houver UnitOfWork, a transação pode não ser gerenciada automaticamente.
    public override async Task ExecuteAsync(ReportGenerationArgs args)
    {
        _logger.LogInformation($"Iniciando geração do relatório {args.ReportId} para o usuário {args.RequestingUserId}...");
        try
        {
            // Simula a lógica de geração de relatório
            await _reportService.GenerateAndSaveReportAsync(args.ReportId, args.RequestingUserId);
            _logger.LogInformation($"Relatório {args.ReportId} gerado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Falha ao gerar relatório {args.ReportId}.");
            // O Hangfire/ABP irá lidar com retries com base na configuração.
            // Pode-se lançar uma exceção customizada para controlar o comportamento de retry.
            throw;
        }
    }
}
// IReportService seria um serviço definido pelo EAF.
public interface IReportService : ITransientDependency { Task GenerateAndSaveReportAsync(Guid reportId, long userId); }
public class ReportService : IReportService { public async Task GenerateAndSaveReportAsync(Guid reportId, long userId) { /* ... */ await Task.Delay(5000); } }
```

### 2.2. Argumentos do Job

Os argumentos passados para um job devem ser serializáveis (geralmente para JSON). É uma boa prática usar classes DTO simples para os argumentos. Evite passar entidades EF Core ou objetos complexos com referências circulares.

### 2.3. Enfileirando um Job

O serviço `IBackgroundJobManager` é usado para enfileirar jobs.

**Exemplo EAF (Enfileirando `ReportGeneratorJob` de um Serviço de Aplicação)**:
```csharp
// Em um Serviço de Aplicação EAF
using Abp.Application.Services;
using Abp.BackgroundJobs;
using Abp.Runtime.Session; // Para IAbpSession
using System;
using System.Threading.Tasks;

public class ReportRequestAppService : ApplicationService, IReportRequestAppService // Supondo que IReportRequestAppService existe
{
    private readonly IBackgroundJobManager _backgroundJobManager;

    public ReportRequestAppService(IBackgroundJobManager backgroundJobManager)
    {
        _backgroundJobManager = backgroundJobManager;
    }

    public async Task RequestReportGenerationAsync(Guid reportIdToGenerate)
    {
        if (AbpSession.UserId == null)
        {
            throw new Abp.UI.UserFriendlyException("Usuário não logado.");
        }

        var jobArgs = new ReportGenerationArgs
        {
            ReportId = reportIdToGenerate,
            RequestingUserId = AbpSession.UserId.Value
        };

        // Enfileira o job para execução o mais rápido possível
        await _backgroundJobManager.EnqueueAsync<ReportGeneratorJob, ReportGenerationArgs>(jobArgs);

        // Também é possível enfileirar com prioridade e/ou delay:
        // await _backgroundJobManager.EnqueueAsync<ReportGeneratorJob, ReportGenerationArgs>(
        //     jobArgs,
        //     BackgroundJobPriority.High, // Prioridade (Normal, Low, High, AboveNormal, BelowNormal)
        //     TimeSpan.FromMinutes(1)    // Delay antes da primeira tentativa
        // );
    }
}
public interface IReportRequestAppService : IApplicationService { Task RequestReportGenerationAsync(Guid reportIdToGenerate); }
```

### 2.4. `DefaultBackgroundJob`

Para tarefas muito simples que não requerem uma classe de job dedicada, o ABP oferece o `DefaultBackgroundJob`. Ele pode executar uma `Action<TArgs>` ou `Func<TArgs, Task>`. No entanto, para clareza e testabilidade, geralmente é melhor criar classes de job dedicadas para a maioria dos casos de uso no EAF.

## 3. Recurring Jobs (Tarefas Recorrentes)

Tarefas recorrentes são executadas repetidamente em um cronograma definido (usando expressões CRON). O ABP integra-se com o Hangfire para fornecer essa funcionalidade através do `IRecurringJobManager`.

*   **Agendamento**: Jobs recorrentes são tipicamente registrados no método `PostInitialize` de um módulo ABP/EAF.
*   **Exemplo EAF (Registrando um Job Recorrente de Limpeza)**:
    ```csharp
    // No método PostInitialize de um módulo EAF (e.g., EafWebCoreModule)
    using Abp.BackgroundJobs;
    using Hangfire;
    using System;
    using Abp.Dependency; // Para IocManager

    public class EafWebCoreModule : AbpModule // Exemplo de nome de módulo
    {
        // ... outros métodos do módulo ...
        public override void PostInitialize()
        {
            var recurringJobManager = IocManager.Resolve<IRecurringJobManager>();

            // Registra um job para rodar diariamente às 2:00 AM UTC
            recurringJobManager.AddOrUpdate<MyDailyCleanupJob>(
                "DailyCleanupService", // Nome único para o job recorrente
                job => job.ExecuteAsync(null), // Método a ser executado, passando null se args não são usados
                Cron.Daily(2, 0),      // Expressão CRON (2:00 AM)
                TimeZoneInfo.Utc       // Especifica o fuso horário para o CRON
            );
        }
    }

    // Definição do MyDailyCleanupJob (similar a ReportGeneratorJob, pode não ter argumentos ou usar um tipo simples)
    public class MyDailyCleanupJob : AsyncBackgroundJob<object>, ITransientDependency
    {
        private readonly ILogger<MyDailyCleanupJob> _logger;
        public MyDailyCleanupJob(ILogger<MyDailyCleanupJob> logger) { _logger = logger; }
        public override async Task ExecuteAsync(object args)
        {
            _logger.LogInformation("Executando job de limpeza diária...");
            await Task.Delay(1000); // Simula trabalho
            _logger.LogInformation("Job de limpeza diária concluído.");
        }
    }
    ```
*   **`ExpiredAuditLogDeleterWorker`**: Um exemplo prático de job recorrente no EAF é o `ExpiredAuditLogDeleterWorker`, que limpa logs de auditoria antigos. Sua configuração e agendamento são detalhados em `docs/architecture/eaf-extensions.md`.

## 4. Integração com Hangfire e Dashboard

*   **Hangfire como Provedor Padrão**: O ABP usa o Hangfire por padrão para persistir e executar background jobs. O Hangfire suporta três tipos de armazenamento, selecionados automaticamente pelo `HangFireConfigurer.ResolveStorageType()`:
    *   **SQL Server**: Quando `Database:Provider` é `SqlServer` ou `MSSQL` (recomendado para produção).
    *   **Redis**: Quando o provider não é SQL Server e `RedisCache:IsEnabled` ou `RedisCache:IsRedisEnabled` está `true`. Utiliza o pacote `Hangfire.Redis.StackExchange` (open source).
    *   **InMemory**: Quando o provider não é SQL Server e Redis não está habilitado, ou quando `Hangfire:IsInMemoryDatabase` está `true`.
*   **Dashboard do Hangfire**: O Hangfire fornece um dashboard web útil para:
    *   Monitorar o status dos jobs (enfileirados, processando, falhados, bem-sucedidos).
    *   Ver o histórico de execuções de cada job, incluindo exceções e durações.
    *   Gerenciar retries para jobs falhados.
    *   Disparar jobs manualmente.
    *   Visualizar jobs recorrentes e seus próximos horários de execução.
*   **Configurações Específicas do EAF para o Dashboard**: O EAF pode ter configurações customizadas para o dashboard do Hangfire, como:
    *   **Autorização**: Controlar quem pode acessar o dashboard. Detalhes sobre isso podem ser encontrados em `docs/architecture/eaf-extensions.md` na seção "Custom Hangfire Configurations in EAF".
    *   O dashboard é geralmente acessível em `/hangfire` na URL da aplicação.

## 5. Considerações Específicas do EAF

*   **Convenções de Nomenclatura/Organização**:
    *   O EAF pode adotar convenções para nomear classes de job (e.g., sufixo `Job` ou `Worker`).
    *   Jobs podem ser organizados em um subdiretório `BackgroundJobs` ou `Workers` dentro dos módulos Core ou Application.
*   **Jobs Base Customizados**: O EAF pode introduzir classes base customizadas para jobs que fornecem logging adicional, manipulação de erro específica do EAF, ou integração com outros serviços EAF.
*   **Configurações de Filas e Retries do Hangfire**:
    *   O EAF pode configurar filas específicas no Hangfire para priorizar ou isolar a execução de certos tipos de jobs.
    *   Políticas de retry para jobs falhados podem ser customizadas.
    *   Consulte `docs/architecture/eaf-extensions.md` para detalhes sobre essas configurações avançadas do Hangfire no EAF.

## 6. Melhores Práticas para Background Jobs no EAF

*   **Idempotência**: Projete seus jobs para serem idempotentes sempre que possível. Isso significa que executar o job múltiplas vezes com os mesmos argumentos terá o mesmo resultado que executá-lo uma vez. Isso é crucial devido a possíveis retries automáticos.
*   **Tratamento de Falhas e Retries**:
    *   O Hangfire/ABP tentará automaticamente reexecutar jobs que falham. Entenda a política de retry padrão e customize-a se necessário.
    *   Use `try-catch` dentro do método `ExecuteAsync` para lidar com exceções específicas e logar informações úteis.
    *   Considere se uma falha deve resultar em retry ou se o job deve falhar permanentemente (e.g., lançando `new BackgroundJobExecutionException("Mensagem", tryLater: false)` para evitar retries automáticos, se a versão do ABP/Hangfire suportar `tryLater`).
*   **Serialização de Argumentos**:
    *   Mantenha os argumentos dos jobs simples (tipos primitivos, DTOs simples).
    *   Eles devem ser serializáveis para JSON (o serializador padrão do Hangfire). Evite passar entidades EF Core ou objetos complexos com estado interno ou dependências de DI nos argumentos.
*   **Manter Jobs Curtos e Focados**:
    *   Jobs de longa duração podem monopolizar workers e atrasar outros jobs. Se uma tarefa for muito longa, divida-a em jobs menores ou etapas.
    *   Cada job deve ter uma responsabilidade clara e única.
*   **Unit of Work (UoW)**:
    *   Jobs que herdam de `BackgroundJob<TArgs>` do ABP têm um UoW gerenciado automaticamente para o método `Execute`.
    *   Para `AsyncBackgroundJob<TArgs>` ou `IAsyncBackgroundJob<TArgs>`, o atributo `[UnitOfWork]` é frequentemente usado no método `ExecuteAsync` para garantir o comportamento transacional. Se não for usado, e o job interagir com o banco de dados, o UoW deve ser gerenciado manualmente usando `IUnitOfWorkManager`.
    *   Esteja ciente de que um job pode rodar por um tempo considerável; transações de banco de dados muito longas podem ser problemáticas. Comite transações o mais cedo possível ou use múltiplos UoWs se apropriado.

## 7. Troubleshooting de Problemas Comuns com Background Jobs

*   **Jobs Não Estão Sendo Processados**:
    *   **Servidor Hangfire Não Está Rodando**: Certifique-se de que o servidor Hangfire (`app.UseHangfireServer()`) está configurado e iniciado no seu processo de aplicação web ou em um processo worker dedicado.
    *   **Problemas de Fila**: Se estiver usando filas customizadas, verifique se há workers escutando nessas filas.
    *   **Nenhum Worker Disponível**: Todos os workers podem estar ocupados com jobs de longa duração.
    *   **Problemas de Conexão com o Armazenamento do Hangfire**: Verifique a conectividade com o banco de dados onde o Hangfire armazena seus dados.
    *   **Dashboard do Hangfire**: É a primeira parada para verificar o status dos jobs e dos servidores.
*   **Jobs Falhando Repetidamente**:
    *   **Exceções no Job**: Verifique a aba "Failed" no dashboard do Hangfire para ver os detalhes da exceção e stack trace.
    *   **Dependências Ausentes ou Não Registradas**: Se o job tentar resolver um serviço não registrado na DI.
    *   **Problemas de Configuração**: O job pode depender de configurações que estão ausentes ou incorretas no ambiente onde ele está rodando.
*   **Problemas com Acesso ao Dashboard do Hangfire**:
    *   Consulte a seção de troubleshooting em `docs/architecture/eaf-extensions.md` sobre autorização do dashboard.
    *   Verifique se o middleware do dashboard (`app.UseHangfireDashboard()`) está configurado corretamente.
*   **Problemas com Agendamento de Jobs Recorrentes**:
    *   **Expressão CRON Incorreta**: Valide sua expressão CRON usando uma ferramenta online.
    *   **Fuso Horário (TimeZone)**: Certifique-se de que o `TimeZoneInfo` especificado no `AddOrUpdate` está correto e consistente com suas expectativas para o horário de execução.
    *   **Nome Único do Job**: Cada job recorrente precisa de um ID único. Usar o mesmo ID sobrescreverá um job existente.
*   **Problemas de Serialização de Argumentos**:
    *   **Argumentos Não Serializáveis**: Se os argumentos do job não puderem ser serializados para JSON (e.g., contêm tipos complexos não-DTOs, referências circulares), o enfileiramento do job falhará.
    *   Verifique os logs do Hangfire ou da aplicação no momento do enfileiramento para erros de serialização.

O sistema de background jobs no EAF, com o poder do Hangfire e as abstrações do ABP, é uma ferramenta vital para construir aplicações responsivas e robustas. Um bom entendimento de como definir, enfileirar e monitorar esses jobs é crucial para os desenvolvedores EAF.
