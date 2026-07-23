# Base ASP.NET Boilerplate no EAF

O Enterprise Application Framework (EAF) é fundamentalmente construído sobre o [ASP.NET Boilerplate (ABP)](https://aspnetboilerplate.com/), um framework de aplicação de código aberto robusto e maduro. Esta dependência significa que o EAF herda e utiliza uma vasta gama de funcionalidades e conceitos arquiteturais do ABP, que são cruciais para seu funcionamento e design. Compreender a base do ABP é essencial para trabalhar eficazmente com o EAF.

Este documento aprofunda como o EAF utiliza e, em alguns casos, estende os principais recursos do ABP, fornecendo exemplos e contexto específicos do EAF.

## Conceitos Core do ABP Alavancados pelo EAF

O EAF adota e se beneficia diretamente dos seguintes conceitos e padrões arquiteturais centrais do ASP.NET Boilerplate:

*   **Arquitetura N-Layer (N-Camadas)**: Conforme introduzido na [visão geral da arquitetura](./overview.md), o EAF segue a estrutura N-Layer do ABP.
    *   **Contexto EAF**: Os projetos EAF são tipicamente organizados em `Eaf.Core` (Domínio), `Eaf.Application` (Serviços de Aplicação), `Eaf.Infrastructure` (Persistência, Integrações), e `Eaf.Web` (Apresentação). Esta estrutura espelha a separação de preocupações do ABP.

*   **Modularidade (AbpModule)**: O ABP promove um design modular através da classe `AbpModule`.
    *   **Contexto EAF**: O EAF é composto por múltiplos módulos. Por exemplo, um módulo customizado `Eaf.CustomFeatureModule` pode depender de módulos ABP e outros módulos EAF.
    ```csharp
    // Exemplo de um módulo EAF (Eaf.CustomFeature.Core)
    using Abp.Modules;
    using Abp.Reflection.Extensions;
    using Eaf.Core; // Supondo um módulo core do EAF

    namespace Eaf.CustomFeature
    {
        [DependsOn(typeof(EafCoreModule))] // Exemplo de dependência
        public class EafCustomFeatureCoreModule : AbpModule
        {
            public override void Initialize()
            {
                IocManager.RegisterAssemblyByConvention(typeof(EafCustomFeatureCoreModule).GetAssembly());
            }
        }
    }
    ```

*   **Injeção de Dependência (Dependency Injection)**: O ABP utiliza o Castle Windsor por padrão.
    *   **Contexto EAF**: A DI é usada extensivamente. Serviços, repositórios e outros componentes são injetados nos construtores. O EAF pode estender o logging da DI com `Eaf.Castle.Serilog`.
    ```csharp
    // Exemplo em um Serviço de Aplicação EAF
    public class MyTaskAppService : ApplicationService
    {
        private readonly IRepository<MyTask, int> _myTaskRepository;
        private readonly ICustomEafService _customEafService;

        public MyTaskAppService(
            IRepository<MyTask, int> myTaskRepository,
            ICustomEafService customEafService) // ICustomEafService é um serviço específico do EAF
        {
            _myTaskRepository = myTaskRepository;
            _customEafService = customEafService;
        }

        // ... métodos do serviço ...
    }
    ```

*   **Repositórios e Unidade de Trabalho (Repositories and Unit of Work)**:
    *   **Contexto EAF**: O EAF utiliza `IRepository<TEntity, TPrimaryKey>` para abstração de dados. A Unidade de Trabalho (`UnitOfWork`) é fundamental para a consistência transacional.
    ```csharp
    // Exemplo de uso de IRepository em um Serviço de Aplicação EAF
    public async Task<MyTaskDto> GetTaskAsync(int id)
    {
        var task = await _myTaskRepository.GetAsync(id);
        return ObjectMapper.Map<MyTaskDto>(task);
    }

    [UnitOfWork] // Garante que o método seja transacional
    public async Task CreateTaskAsync(CreateTaskDto input)
    {
        var task = ObjectMapper.Map<MyTask>(input);
        await _myTaskRepository.InsertAsync(task);
        // Outras operações dentro da mesma transação...
    }
    ```
    *   Métodos de Serviços de Aplicação são `UnitOfWork` por padrão. A anotação explícita `[UnitOfWork]` permite configurar propriedades como `IsTransactional = false` ou níveis de isolamento.

*   **Entidades (Entities) e Serviços de Domínio (Domain Services)**:
    *   **Contexto EAF**: Entidades EAF herdam de classes base do ABP como `Entity<T>`, `AuditedEntity<T>`, ou `FullAuditedEntity<T>`. Serviços de Domínio encapsulam lógica de negócios que não pertence a uma única entidade.
    ```csharp
    // Exemplo de uma Entidade EAF (MyProject.Core)
    using Abp.Domain.Entities;
    using Abp.Domain.Entities.Auditing;
    using System;

    public class MyProjectItem : FullAuditedEntity<Guid> // Herda de FullAuditedEntity para auditoria completa
    {
        public string Name { get; set; }
        public string Description { get; set; }
        // Outras propriedades específicas do EAF
    }

    // Exemplo de um Serviço de Domínio EAF (MyProject.Core)
    public interface IProjectManager : IDomainService
    {
        Task AssignUserToProjectAsync(Guid projectId, long userId);
    }

    public class ProjectManager : DomainService, IProjectManager
    {
        private readonly IRepository<MyProjectItem, Guid> _projectRepository;
        private readonly IRepository<User, long> _userRepository; // Supondo uma entidade User do AbpZero

        public ProjectManager(
            IRepository<MyProjectItem, Guid> projectRepository,
            IRepository<User, long> userRepository)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
        }

        public async Task AssignUserToProjectAsync(Guid projectId, long userId)
        {
            var project = await _projectRepository.GetAsync(projectId);
            var user = await _userRepository.GetAsync(userId);
            // Lógica de negócios para associar usuário ao projeto
            // ...
            // Pode disparar eventos de domínio aqui
            // EventBus.Default.Trigger(new UserAssignedToProjectEventData(projectId, userId));
        }
    }
    ```

*   **Serviços de Aplicação (Application Services) e DTOs (Data Transfer Objects)**:
    *   **Contexto EAF**: Serviços de Aplicação orquestram chamadas aos serviços de domínio e repositórios. DTOs são usados para passar dados.
    ```csharp
    // Exemplo de DTO EAF (MyProject.Application)
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using System;

    [AutoMap(typeof(MyProjectItem))] // Mapeamento AutoMapper para a entidade MyProjectItem
    public class MyProjectItemDto : AuditedEntityDto<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    // Exemplo de um Serviço de Aplicação EAF (MyProject.Application)
    public class ProjectAppService : AsyncCrudAppService<MyProjectItem, MyProjectItemDto, Guid, GetAllProjectItemsInput, CreateProjectItemDto, UpdateProjectItemDto>, IProjectAppService
    {
        private readonly IProjectManager _projectManager; // Injetando um serviço de domínio EAF

        public ProjectAppService(
            IRepository<MyProjectItem, Guid> repository,
            IProjectManager projectManager) : base(repository)
        {
            _projectManager = projectManager;
        }

        // Método customizado que usa o serviço de domínio
        public async Task AssignUserAsync(AssignUserToProjectDto input)
        {
            await _projectManager.AssignUserToProjectAsync(input.ProjectId, input.UserId);
        }
        // GetAll, Create, Update, Delete são herdados de AsyncCrudAppService
    }
    ```

*   **Auditoria (Auditing) e Logging**:
    *   **Contexto EAF**: Entidades EAF como `MyProjectItem` (acima) herdam de `AuditedEntity` ou `FullAuditedEntity` para obter rastreamento automático de criação, modificação e exclusão.
    *   O EAF utiliza o sistema de logging do ABP, que pode ser estendido com implementações como `Eaf.Log4NetServiceBus` (para enviar logs ao Azure Service Bus) ou `Eaf.Castle.Serilog` (para integração com Serilog), configuradas nos módulos de inicialização do EAF.

*   **Gerenciamento de Sessão (IAbpSession)**:
    *   **Contexto EAF**: `IAbpSession` é usado para obter informações do usuário e tenant logados.
    ```csharp
    // Em um serviço EAF
    public class SomeEafService : ITransientDependency
    {
        private readonly IAbpSession _abpSession;
        private readonly ILogger _logger;

        public SomeEafService(IAbpSession abpSession, ILogger logger)
        {
            _abpSession = abpSession;
            _logger = logger;
        }

        public void DoSomething()
        {
            if (_abpSession.UserId.HasValue)
            {
                _logger.Info($"Operação realizada pelo usuário: {_abpSession.UserId.Value}");
            }
            if (_abpSession.TenantId.HasValue)
            {
                // Lógica específica para o tenant
                _logger.Info($"Operação no contexto do tenant: {_abpSession.TenantId.Value}");
            }
        }
    }
    ```

*   **Autorização (Authorization)**:
    *   **Contexto EAF**: O EAF define suas próprias permissões e as utiliza para proteger APIs e funcionalidades. A autenticação pode ser integrada com Azure AD (`Eaf.Middleware.AzureActiveDirectoryModule`) ou LDAP (`Eaf.Middleware.LdapModule`).
    ```csharp
    // Definição de Permissão EAF (em um AuthorizationProvider no módulo Core)
    public class MyProjectAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            var projectsNode = context.CreatePermission("Eaf.MyProject", L("MyProject"));
            projectsNode.CreateChildPermission("Eaf.MyProject.CanCreateItems", L("CanCreateProjectItems"));
            projectsNode.CreateChildPermission("Eaf.MyProject.CanDeleteItems", L("CanDeleteProjectItems"));
        }
        private static LocalizableString L(string name) => new LocalizableString(name, EafConsts.LocalizationSourceName);
    }

    // Uso em um Serviço de Aplicação EAF
    [AbpAuthorize("Eaf.MyProject.CanCreateItems")]
    public async Task CreateProjectItemAsync(CreateProjectItemDto input)
    {
        // ... lógica de criação ...
    }
    ```

*   **Gerenciamento de Configurações (Setting Management)**:
    *   **Contexto EAF**: O EAF define configurações específicas da aplicação (ex: URL de um serviço externo, configurações do Key Vault) que podem ser gerenciadas pelo sistema de settings do ABP e acessadas via `ISettingManager`.
    ```csharp
    // Exemplo de definição de uma configuração (em um SettingProvider no módulo Core)
    public class MyProjectSettingProvider : SettingProvider
    {
        public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
        {
            return new[]
            {
                new SettingDefinition(
                    "Eaf.MyProject.ExternalServiceUrl", // Nome da configuração
                    "http://default.service.url",      // Valor padrão
                    L("ExternalServiceUrlSetting"),
                    scopes: SettingScopes.Application | SettingScopes.Tenant, // Aplicável a nível de aplicação e tenant
                    isVisibleToClients: false)
            };
        }
        // ... L() helper ...
    }

    // Acessando a configuração
    var externalUrl = await SettingManager.GetSettingValueAsync("Eaf.MyProject.ExternalServiceUrl");
    ```

*   **EventBus / Eventos de Domínio (Domain Events)**:
    *   **Contexto EAF**: Usado para comunicação desacoplada. Por exemplo, após um novo usuário ser registrado através do `Eaf.Middleware.AzureActiveDirectoryModule`, um evento de domínio pode ser disparado para outras partes do sistema reagirem.
    ```csharp
    // Exemplo de um Evento de Domínio EAF
    public class NewUserRegisteredEafEventData : EventData
    {
        public long UserId { get; }
        public string EmailAddress { get; }
        public NewUserRegisteredEafEventData(long userId, string emailAddress)
        {
            UserId = userId;
            EmailAddress = emailAddress;
        }
    }

    // Disparando o evento (ex: em um serviço após registro via Azure AD)
    // await _eventBus.TriggerAsync(new NewUserRegisteredEafEventData(user.Id, user.EmailAddress));

    // Manipulador do evento
    public class NewUserNotifier : IAsyncEventHandler<NewUserRegisteredEafEventData>, ITransientDependency
    {
        public async Task HandleEventAsync(NewUserRegisteredEafEventData eventData)
        {
            // Enviar email de boas-vindas, etc.
        }
    }
    ```

*   **Multi-tenancy**:
    *   **Contexto EAF**: O EAF é projetado para ser multi-tenant. A configuração do tenant (ex: string de conexão, provedores de identidade externos como `TenantBasedOpenIdConnectExternalLoginInfoProvider`) é gerenciada pelo EAF. `IAbpSession.TenantId` é crucial.
    *   O EAF pode ter uma tela de gerenciamento de tenants onde configurações específicas por tenant são definidas, incluindo se o tenant usará um IdP dedicado (Azure AD, LDAP) ou o sistema de usuários local do EAF/ABP.

## Módulos Chave do ABP Utilizados pelo EAF

O EAF integra e depende de vários módulos fundamentais do ASP.NET Boilerplate. Abaixo, alguns exemplos com contexto EAF:

*   **`AbpZeroCoreModule`**: Essencial para o EAF. Fornece a base para usuários, papéis, tenants, permissões. O EAF estende essas entidades (ex: `EafUser`, `EafTenant`) para adicionar propriedades específicas.
*   **`AbpAutoMapperModule`**: Usado extensivamente no EAF para mapear entre entidades EAF (ex: `MyProjectItem`) e DTOs EAF (ex: `MyProjectItemDto`). As configurações de AutoMapper específicas do EAF são definidas em classes `EafCustomDtoMapper` ou perfis AutoMapper.
*   **`AbpMailKitModule`**: Utilizado para funcionalidades de envio de e-mail no EAF, como notificações de registro ou alertas.
*   **`AbpAspNetCoreModule`**: Fundamental para a integração com o ASP.NET Core, permitindo que os módulos e serviços do EAF operem no ambiente web.
*   **`AbpHangfireAspNetCoreModule`**: O EAF utiliza o Hangfire para tarefas em segundo plano, como o `ExpiredAuditLogDeleterWorker` (mencionado em `eaf-extensions.md`), que é um job específico do EAF para limpar logs de auditoria antigos.
*   **`AbpAspNetCoreSignalRModule`**: Pode ser usado no EAF para notificações em tempo real para clientes conectados, por exemplo, sobre o status de uma operação de longa duração.
*   **Módulos de Caching**: O EAF utiliza as abstrações de cache do ABP (`ICacheManager`, `ITypedCache`). Além dos provedores padrão do ABP (como Redis), o EAF introduz implementações customizadas como `Eaf.SqlServerCacheModule` e `Eaf.SqliteCache`, que se registram como provedores de cache no sistema ABP.
    ```csharp
    // Exemplo de uso do ICacheManager no EAF
    public class MyDataService : ITransientDependency
    {
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<SomeData, int> _someDataRepository;

        public MyDataService(ICacheManager cacheManager, IRepository<SomeData, int> someDataRepository)
        {
            _cacheManager = cacheManager;
            _someDataRepository = someDataRepository;
        }

        public async Task<SomeData> GetSomeDataAsync(int id)
        {
            // Tenta obter do cache 'MyCache'
            return await _cacheManager.GetCache("MyCache")
                .GetAsync(id.ToString(), async () => await _someDataRepository.GetAsync(id));
        }
    }
    ```

## Referência à Documentação do ABP

Para um mergulho profundo nas funcionalidades específicas do ASP.NET Boilerplate, como sua arquitetura, módulos e conceitos centrais, a [documentação oficial do ABP](https://aspnetboilerplate.com/Pages/Documents) é a fonte primária de informação.

## Fim de Suporte do ASP.NET Boilerplate

É importante notar que o projeto ASP.NET Boilerplate anunciou o fim de seu suporte (end-of-life) para **Maio de 2026**. Esta informação é crucial para o planejamento de longo prazo e futuras evoluções do EAF.

Compreender como o EAF se baseia no ASP.NET Boilerplate é o primeiro passo para entender a arquitetura mais ampla do EAF e como suas customizações e extensões se encaixam no framework subjacente.
