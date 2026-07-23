# Auditoria no EAF (Enterprise Application Foundation)

## 1. Introdução à Auditoria no EAF

A auditoria é um aspecto crítico de aplicações modernas, fornecendo um registro de eventos e alterações que ocorrem no sistema. Esses registros são essenciais para:

*   **Responsabilidade (Accountability)**: Rastrear quem fez o quê e quando.
*   **Análise de Segurança**: Detectar atividades suspeitas ou não autorizadas.
*   **Debugging e Diagnóstico**: Entender a sequência de eventos que levaram a um estado particular ou erro.
*   **Conformidade (Compliance)**: Atender a requisitos regulatórios que exigem trilhas de auditoria.

O EAF (Enterprise Application Foundation) é uma plataforma open source que aproveita o abrangente sistema de auditoria fornecido pelo ASP.NET Boilerplate (ABP), que é uma parte central de módulos como `Abp.ZeroCore.EntityFrameworkCore` (ou equivalentes, dependendo da versão exata e configuração do EAF). Este sistema automatiza grande parte do processo de auditoria, ao mesmo tempo que oferece flexibilidade para customização.

## 2. Funcionalidades de Auditoria do ABP Utilizadas pelo EAF

O EAF utiliza nativamente as robustas funcionalidades de auditoria do ABP:

### 2.1. Logging Automático de Auditoria de Interação

Por padrão, o ABP audita automaticamente chamadas para:

*   Serviços de Aplicação (Application Services)
*   Métodos de API Controllers MVC/ASP.NET Core
*   Outros métodos e classes marcados com o atributo `[Audited]`

As seguintes informações são tipicamente registradas para cada requisição/interação auditada:

*   **Informações do Usuário**: `UserId` e `TenantId` do usuário que realizou a ação (obtidos via `IAbpSession`).
*   **Informações do Cliente**: Endereço IP, nome do navegador, e versão do cliente.
*   **Ação Executada**: Nome do serviço/controller e método chamado.
*   **Parâmetros da Ação**: Argumentos passados para o método (pode ser configurável para não logar dados sensíveis).
*   **Tempo de Execução**: Data e hora do início da execução e a duração.
*   **Erros**: Exceções, se alguma ocorrer durante a execução.
*   **Dados Customizados**: O EAF pode adicionar dados específicos a cada entrada de log de auditoria (ver seção 3.3).

### 2.2. Auditoria de Entidades

O ABP fornece interfaces e classes base para auditar automaticamente a criação, modificação e exclusão de entidades.

*   **Interfaces de Auditoria**:
    *   `IHasCreationTime`, `ICreationAudited`: Para rastrear quando e por quem uma entidade foi criada. Propriedades: `CreationTime`, `CreatorUserId`.
    *   `IHasModificationTime`, `IModificationAudited`: Para rastrear quando e por quem uma entidade foi modificada pela última vez. Propriedades: `LastModificationTime`, `LastModifierUserId`.
    *   `IHasDeletionTime`, `IDeletionAudited`: Para rastrear quando e por quem uma entidade foi deletada (para soft delete). Propriedades: `DeletionTime`, `DeleterUserId`, `IsDeleted`.
    *   `ISoftDelete`: Interface marcadora para indicar que uma entidade usa soft delete (apenas marca como deletada em vez de remover do banco).

*   **Classes Base de Entidade Auditada**:
    *   `AuditedEntity<TPrimaryKey>`: Implementa `ICreationAudited` e `IModificationAudited`.
    *   `FullAuditedEntity<TPrimaryKey>`: Implementa `ICreationAudited`, `IModificationAudited`, e `IDeletionAudited` (para soft delete).
    *   `AuditedAggregateRoot<TPrimaryKey>` e `FullAuditedAggregateRoot<TPrimaryKey>`: Versões para raízes de agregado.

*   **Exemplo EAF (Entidade `EafProject` Auditada)**:
    Suponha uma entidade `EafProject` em um módulo EAF.
    ```csharp
    // No projeto Eaf.MyModule.Core
    using Abp.Domain.Entities.Auditing;
    using System;
    using System.ComponentModel.DataAnnotations;

    public class EafProject : FullAuditedEntity<Guid> // Herda de FullAuditedEntity para auditoria completa
    {
        [Required]
        [StringLength(255)]
        public string ProjectName { get; set; }

        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? TargetCompletionDate { get; set; }

        // Construtor
        protected EafProject() { } // Necessário para EF Core

        public EafProject(Guid id, string projectName, DateTime startDate) : base(id)
        {
            ProjectName = projectName;
            StartDate = startDate;
        }
    }
    ```
    Quando instâncias de `EafProject` são inseridas, atualizadas ou deletadas (soft delete) através dos repositórios do ABP, as propriedades como `CreationTime`, `CreatorUserId`, `LastModificationTime`, `LastModifierUserId`, `IsDeleted`, `DeleterUserId`, e `DeletionTime` são preenchidas automaticamente pelo ABP.

### 2.3. Rastreamento de Alterações de Entidade (Entity Change Tracking)

Para entidades auditadas (especialmente aquelas gerenciadas pelo `Abp.ZeroCore`), o ABP pode registrar automaticamente as alterações feitas nas propriedades da entidade. Cada alteração de propriedade (nome da propriedade, valor original, novo valor) é armazenada na tabela `AbpEntityPropertyChanges`, vinculada ao `AbpEntityChange` que, por sua vez, está ligado ao `AbpAuditLog`.

Isso é particularmente útil para entender o histórico de modificações de uma entidade específica.

### 2.4. `IAbpSession` para Informações do Usuário

O `IAbpSession` do ABP é crucial para a auditoria, pois fornece o `UserId` e `TenantId` do usuário atual. Esses IDs são automaticamente registrados nos logs de auditoria e nas propriedades de auditoria das entidades (`CreatorUserId`, `LastModifierUserId`, `DeleterUserId`).

## 3. Configurações e Extensões de Auditoria Específicas do EAF

O EAF pode customizar e estender o comportamento de auditoria do ABP:

### 3.1. Habilitando/Desabilitando Auditoria

*   **Globalmente**: A auditoria pode ser habilitada ou desabilitada globalmente no método `PreInitialize` de um módulo EAF (geralmente o módulo Core ou Web).
    ```csharp
    // Em EafMyModuleCoreModule.cs ou similar
    public override void PreInitialize()
    {
        Configuration.Auditing.IsEnabled = true; // Habilita por padrão
        Configuration.Auditing.IsEnabledForAnonymousUsers = false; // Desabilita para usuários anônimos

        // O EAF pode ter suas próprias configurações que controlam isso,
        // por exemplo, lendo de appsettings.json
        // var eafAuditSettings = Configuration.GetSection("Eaf:Auditing").Get<EafAuditSettings>();
        // if (eafAuditSettings != null) {
        //     Configuration.Auditing.IsEnabled = eafAuditSettings.IsEnabled;
        // }
    }
    ```

*   **Por Serviço/Método (Atributos)**:
    *   `[Audited]`: Pode ser usado em uma classe de serviço ou método para forçar a auditoria, mesmo que esteja desabilitada por padrão ou para um método não convencional.
    *   `[DisableAuditing]`: Impede a auditoria para uma classe ou método específico.

    ```csharp
    // Exemplo em um Serviço de Aplicação EAF
    using Abp.Application.Services;
    using Abp.Auditing;

    public class MySensitiveDataAppService : ApplicationService
    {
        [DisableAuditing] // Não auditar este método específico devido a dados sensíveis nos parâmetros/retorno
        public async Task<SensitiveDto> GetSensitiveDataAsync(int id)
        {
            // ... lógica ...
            return new SensitiveDto();
        }

        [Audited("EAF.HighValueOperation")] // Força auditoria e pode dar um nome customizado à ação
        public async Task PerformCriticalUpdateAsync(UpdateInputDto input)
        {
            // ... lógica crítica ...
        }
    }
    ```

### 3.2. `ExpiredAuditLogDeleterWorker`

Conforme detalhado em `docs/architecture/eaf-extensions.md`, o EAF inclui o `ExpiredAuditLogDeleterWorker`. Este job em segundo plano é responsável por excluir periodicamente registros de auditoria antigos, ajudando a gerenciar o tamanho do banco de dados e a performance. Consulte a documentação de extensões para detalhes de configuração e implementação.

### 3.3. Adicionando Dados Customizados à Auditoria

O EAF pode precisar adicionar informações contextuais específicas aos logs de auditoria que não são capturadas por padrão. Isso pode ser feito implementando `IAuditInfoProvider` ou modificando `AuditInfo` em um ponto apropriado.

*   **Exemplo com `IAuditInfoProvider`**:
    Suponha que o EAF queira adicionar um `CorrelationId` ou `SessionId` customizado a todos os logs de auditoria.
    ```csharp
    // Em um módulo EAF (e.g., Eaf.Web.Core)
    using Abp.Auditing;
    using Abp.Dependency;
    using Microsoft.AspNetCore.Http; // Para acessar HttpContext
    using System.Collections.Generic; // Para Dictionary

    public class EafAuditInfoProvider : IAuditInfoProvider, ITransientDependency
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // IHttpContextAccessor pode ser null fora do contexto de uma requisição web
        public EafAuditInfoProvider(IHttpContextAccessor httpContextAccessor = null)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Fill(AuditInfo auditInfo)
        {
            // Adiciona dados customizados que serão serializados como JSON em AuditInfo.CustomData
            if (auditInfo.CustomData == null)
            {
                // auditInfo.CustomData é string, então inicializamos como um objeto JSON string anônimo ou um Dictionary serializado
                // Para simplicidade, vamos assumir que podemos adicionar diretamente ou que temos um helper para JSON
            }

            var correlationId = _httpContextAccessor?.HttpContext?.Request?.Headers["X-Correlation-ID"].ToString();
            if (!string.IsNullOrEmpty(correlationId))
            {
                // Se AuditInfo.CustomData for para ser um JSON string:
                // auditInfo.CustomData = AddJsonEntry(auditInfo.CustomData, "CorrelationId", correlationId);
                // Ou se for um Dictionary<string, object> que será serializado depois:
                // (Esta parte depende de como o CustomData é tratado internamente pelo ABP ou EAF customizações)
                // Por agora, vamos assumir que é um campo de texto e concatenamos ou usamos um formato estruturado.
                // Para o exemplo, vamos apenas logar, a implementação real precisaria de uma estratégia de serialização.
                // Logger.Debug($"CorrelationId para auditoria: {correlationId}");
                // Para efetivamente adicionar ao CustomData, seria necessário um mecanismo como:
                // auditInfo.CustomData = auditInfo.CustomData ?? "";
                // auditInfo.CustomData += $"CorrelationId: {correlationId}; ";
                // Uma abordagem melhor seria usar um Dictionary e serializá-lo no final,
                // ou se o ABP permitir um Dictionary<string, object> diretamente.
                // Supondo que CustomData possa ser um objeto que será serializado:
                var customDataObject = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(auditInfo.CustomData)) {
                    // Tentar desserializar se já houver dados, ou tratar como append.
                    // Para este exemplo, vamos apenas adicionar.
                }
                customDataObject["CorrelationId"] = correlationId;
                // (Serializar customDataObject para auditInfo.CustomData seria o passo final,
                // ou atribuir diretamente se o tipo permitir)
            }

            // Exemplo: Adicionar um identificador de máquina ou instância
            // customDataObject["MachineName"] = Environment.MachineName;
            // auditInfo.CustomData = Newtonsoft.Json.JsonConvert.SerializeObject(customDataObject); // Exemplo de serialização
        }
    }
    ```
    Esta implementação de `IAuditInfoProvider` precisa ser registrada na DI. O ABP a chamará ao construir o `AuditInfo`. O `EafAuditInfoProvider` seria adicionado à lista `Configuration.Auditing.InfoProviders` no método `PreInitialize` de um módulo. *Nota: A manipulação exata de `AuditInfo.CustomData` pode variar; o exemplo acima ilustra a intenção. A serialização para JSON é uma abordagem comum.*

## 4. Acessando e Consultando Logs de Auditoria no EAF

Os logs de auditoria são armazenados na tabela `AbpAuditLogs` (e tabelas relacionadas como `AbpEntityChanges`).

### 4.1. Consultando Logs Programaticamente

Pode-se injetar `IRepository<AuditLog, long>` para acesso direto ou `IAuditLogManager` (se o EAF fornecer uma abstração de nível superior).

*   **Exemplo com `IRepository<AuditLog, long>`**:
    ```csharp
    // Em um Serviço de Aplicação EAF
    using Abp.Application.Services;
    using Abp.Auditing;
    using Abp.Domain.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore; // Para ToListAsync e IQueryable

    public class AuditLogViewerAppService : ApplicationService, IAuditLogViewerAppService
    {
        private readonly IRepository<AuditLog, long> _auditLogRepository;

        public AuditLogViewerAppService(IRepository<AuditLog, long> auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<List<AuditLogDto>> GetAuditLogsByUserAsync(long userId, DateTime startDate, DateTime endDate)
        {
            var query = _auditLogRepository.GetAll()
                .Where(al => al.UserId == userId &&
                               al.ExecutionTime >= startDate &&
                               al.ExecutionTime <= endDate)
                .OrderByDescending(al => al.ExecutionTime);

            var auditLogs = await query.ToListAsync();
            return ObjectMapper.Map<List<AuditLogDto>>(auditLogs); // Mapear para DTOs
        }

        public async Task<List<AuditLogDto>> GetAuditLogsByServiceNameAsync(string serviceName)
        {
            var query = _auditLogRepository.GetAll()
                .Where(al => al.ServiceName.Contains(serviceName)) // Cuidado com 'Contains' em grandes datasets
                .OrderByDescending(al => al.ExecutionTime)
                .Take(100); // Limitar resultados

            var auditLogs = await query.ToListAsync();
            return ObjectMapper.Map<List<AuditLogDto>>(auditLogs);
        }
    }
    ```

### 4.2. UI para Visualização de Logs de Auditoria

Aplicações baseadas no `Abp.ZeroCore` (que o EAF provavelmente utiliza) geralmente incluem uma UI administrativa para visualizar logs de auditoria. Esta interface normalmente permite:

*   Filtrar logs por intervalo de datas, nome de usuário, serviço, duração da execução, e status de erro.
*   Visualizar detalhes de um log de auditoria, incluindo parâmetros e alterações de entidade (se rastreadas).
*   Esta funcionalidade é comumente encontrada em uma seção como "Administração" > "Logs de Auditoria" na UI do EAF.

## 5. Tópicos Avançados de Auditoria e Melhores Práticas

*   **O Que Auditar**:
    *   Concentre-se em operações críticas de negócios, alterações de dados sensíveis, e eventos de segurança.
    *   Evite auditar operações de leitura de alta frequência (`GET` requests) que não modificam o estado, a menos que seja um requisito específico.
    *   Tenha cuidado ao auditar parâmetros de métodos ou propriedades de entidade que possam conter Dados Pessoais Identificáveis (PII) ou outros dados sensíveis, especialmente se os logs de auditoria forem amplamente acessíveis. Considere anonimização ou não logar campos específicos.
*   **Performance**:
    *   Auditoria extensiva, especialmente o rastreamento de alterações de entidade para muitas entidades ou propriedades, pode impactar a performance da escrita no banco de dados.
    *   A tabela `AbpAuditLogs` pode crescer rapidamente. O uso de `ExpiredAuditLogDeleterWorker` ou estratégias de arquivamento é crucial.
*   **Revisão de Logs**: Defina um processo para revisar periodicamente os logs de auditoria, especialmente para atividades suspeitas ou erros.
*   **Convenções EAF**: Siga quaisquer convenções específicas do EAF para nomear ações auditadas (`[Audited("MyCustomAction")]`) ou para adicionar dados customizados, garantindo consistência.

## 6. Troubleshooting de Problemas Comuns de Auditoria

*   **Auditoria Não Funciona para um Serviço/Método**:
    *   Verifique se `Configuration.Auditing.IsEnabled` está `true`.
    *   Certifique-se de que a classe ou método não está marcado com `[DisableAuditing]`.
    *   Para auditoria automática, o método geralmente precisa ser parte de um Serviço de Aplicação, MVC Controller, ou marcado com `[Audited]`.
    *   Se for para usuários anônimos, `Configuration.Auditing.IsEnabledForAnonymousUsers` deve ser `true`.
*   **Logs de Auditoria Não Capturam Informações Esperadas**:
    *   **Informações do Usuário**: `IAbpSession` está retornando `null` para `UserId` ou `TenantId`? Isso pode acontecer se a requisição não estiver autenticada ou o contexto da sessão não for estabelecido corretamente.
    *   **Parâmetros**: Por padrão, o ABP tenta serializar os parâmetros. Se um parâmetro não é serializável ou é muito grande, pode ser omitido ou causar erro. Verifique `AuditLog.Parameters`.
    *   **Alterações de Entidade**: A funcionalidade de rastreamento de alterações de entidade (`EntityChangeSet`) está habilitada para a entidade em questão? A entidade implementa as interfaces de auditoria corretas?
    *   **Dados Customizados**: A implementação de `IAuditInfoProvider` está correta e registrada na DI? Está sendo chamada? A lógica de serialização/atribuição para `AuditInfo.CustomData` está funcionando como esperado?
*   **Problemas com `ExpiredAuditLogDeleterWorker`**:
    *   Consulte a seção de troubleshooting em `docs/architecture/eaf-extensions.md` para este worker.
    *   Verifique se o job está agendado e rodando no Hangfire.
    *   Confirme se as configurações de idade de expiração e batch size estão corretas.
    *   A conta de usuário do banco de dados usada pela aplicação precisa de permissões de `DELETE` na tabela `AbpAuditLogs`.

A auditoria é uma funcionalidade poderosa no EAF. Compreender como ela funciona e como configurá-la corretamente é vital para a manutenção e segurança da aplicação.
