# Domain-Driven Design (DDD) no EAF

O Domain-Driven Design (DDD) é uma abordagem para o desenvolvimento de software que se concentra em modelar o software para corresponder a um domínio ou área de negócios. O ASP.NET Boilerplate (ABP), a fundação do Enterprise Application Framework (EAF), é fortemente influenciado pelos princípios do DDD. Consequentemente, o EAF adota e estende esses princípios para construir aplicações complexas e centradas no domínio.

A aplicação do DDD ajuda o EAF a criar sistemas que são mais alinhados com as necessidades do negócio, mais fáceis de manter e evoluir, e que possuem uma linguagem comum entre desenvolvedores e especialistas do domínio.

## Estrutura de Camadas (Layered Architecture)

O DDD promove uma arquitetura em camadas para separar as responsabilidades dentro de uma aplicação. O EAF adere à arquitetura N-Layer do ABP:

1.  **Camada de Apresentação (Presentation Layer)**:
    *   **Responsabilidade**: Interação com o usuário ou sistemas externos. Exibe informações e captura comandos do usuário.
    *   **No EAF**: Inclui interfaces de usuário (UI) desenvolvidas com Angular, ASP.NET Core MVC (para áreas administrativas como o `Eaf.Web.Mvc`), e APIs Web (Controllers RESTful, por exemplo, no projeto `Eaf.Web.Host` ou `Eaf.Application.Services` se expostos via API).

2.  **Camada de Aplicação (Application Layer)**:
    *   **Responsabilidade**: Orquestra os casos de uso da aplicação. Não contém lógica de negócios, mas coordena as tarefas, delega o trabalho para objetos de domínio e serviços de domínio. Move dados entre a camada de apresentação e a camada de domínio.
    *   **No EAF**: Consiste principalmente em **Serviços de Aplicação (Application Services)**. Estes serviços utilizam **Objetos de Transferência de Dados (DTOs)** para entrada e saída. A validação de entrada também é uma preocupação desta camada. O projeto principal para esta camada no EAF é geralmente `Eaf.Middleware.Application` (ou similar, dependendo da estrutura exata do projeto como `*.Application`).

3.  **Camada de Domínio (Domain Layer)**:
    *   **Responsabilidade**: O coração do software. Contém a lógica de negócios, as regras e o estado do domínio. É a camada mais importante e deve ser isolada de preocupações de infraestrutura e apresentação.
    *   **No EAF**: Inclui **Entidades (Entities)**, **Agregados (Aggregates)**, **Objetos de Valor (Value Objects)**, **Serviços de Domínio (Domain Services)**, interfaces de **Repositórios (Repositories)**, e **Eventos de Domínio (Domain Events)**. O projeto central para esta camada no EAF é tipicamente `Eaf.Middleware.Core` (ou similar, como `*.Core` ou `*.Domain`).

4.  **Camada de Infraestrutura (Infrastructure Layer)**:
    *   **Responsabilidade**: Fornece implementações técnicas para as outras camadas, especialmente para as interfaces definidas na camada de domínio (como repositórios). Lida com persistência de dados, logging, envio de e-mails, integração com sistemas externos, etc.
    *   **No EAF**: Inclui implementações concretas de repositórios usando Entity Framework Core (por exemplo, no projeto `Eaf.EntityFrameworkCore`), integrações com serviços como Azure Key Vault (`Eaf.KeyVault`), Azure Service Bus (`Eaf.Log4NetServiceBus`), e outros componentes que lidam com preocupações transversais.

## Principais Blocos de Construção do DDD em EAF

O EAF utiliza os seguintes blocos de construção do DDD, muitos dos quais são fornecidos ou facilitados pelo ABP:

*   **Entidades (Entities)**:
    *   Objetos que possuem uma identidade única que persiste ao longo do tempo e através de diferentes estados. No ABP e EAF, as entidades geralmente herdam de `Entity<TPrimaryKey>` ou `AuditedEntity<TPrimaryKey>` para rastreamento automático.
    *   Exemplos chave no EAF incluem `User`, `Role`, `Tenant` (geralmente localizados em `Eaf.Middleware.Core` ou um projeto de domínio similar), que são fundamentais para o módulo `AbpZeroCoreModule`.
    *   **Exemplo EAF (Entidade Simples)**:
        Suponha um módulo EAF para gerenciar "Projetos Customizados".
        ```csharp
        // No projeto Eaf.CustomProjects.Core
        using Abp.Domain.Entities;
        using Abp.Domain.Entities.Auditing;
        using System;

        public class CustomProject : AuditedEntity<Guid> // Herda de AuditedEntity para auditoria
        {
            public string ProjectName { get; protected set; }
            public string Description { get; protected set; }
            public DateTime StartDate { get; protected set; }
            public DateTime? EndDate { get; protected set; }
            public ProjectStatus Status { get; protected set; }

            // Construtor privado para EF Core e para uso interno pela factory ou AggregateRoot
            protected CustomProject() { }

            public CustomProject(Guid id, string projectName, string description, DateTime startDate) : base(id)
            {
                if (string.IsNullOrWhiteSpace(projectName))
                    throw new ArgumentNullException(nameof(projectName));

                ProjectName = projectName;
                Description = description;
                StartDate = startDate;
                Status = ProjectStatus.Planning; // Estado inicial
            }

            public void UpdateDetails(string newName, string newDescription)
            {
                if (!string.IsNullOrWhiteSpace(newName)) ProjectName = newName;
                if (newDescription != null) Description = newDescription; // Permite limpar a descrição
            }

            public void StartProject()
            {
                if (Status == ProjectStatus.Planning) Status = ProjectStatus.InProgress;
                // Adicionar lógica de domínio, e.g., não pode iniciar se já estiver completo
            }
            // Outros métodos que modificam o estado do projeto...
        }

        public enum ProjectStatus { Planning, InProgress, Completed, Cancelled }
        ```

*   **Agregados (Aggregates)**:
    *   Um agregado é um cluster de entidades e objetos de valor que são tratados como uma única unidade para consistência transacional. Cada agregado tem uma raiz (Aggregate Root), que é a única entidade pela qual o acesso e a modificação do agregado são permitidos.
    *   O ABP suporta este conceito com a classe base `AggregateRoot<TPrimaryKey>`. O EAF utiliza agregados para garantir a consistência das regras de negócios dentro de um limite definido.
    *   **Exemplo EAF (Agregado e Raiz de Agregado)**:
        Continuando com `CustomProject`, se ele gerenciar uma coleção de `ProjectTask` (onde `ProjectTask` é uma entidade), `CustomProject` seria a Raiz do Agregado.
        ```csharp
        // No projeto Eaf.CustomProjects.Core
        using Abp.Domain.Entities;
        using System.Collections.Generic;
        using System.Linq;

        public class CustomProject : AggregateRoot<Guid> // Agora é um AggregateRoot
        {
            // ... propriedades e construtor como antes ...
            public string ProjectName { get; protected set; }
            public ProjectStatus Status { get; protected set; }

            private readonly List<ProjectTask> _tasks = new List<ProjectTask>();
            public IReadOnlyCollection<ProjectTask> Tasks => _tasks.AsReadOnly();

            protected CustomProject() { } // Para EF Core

            public CustomProject(Guid id, string projectName /*...*/) : base(id)
            {
                ProjectName = projectName;
                Status = ProjectStatus.Planning;
            }

            public void AddTask(string taskName, int estimatedHours)
            {
                if (Status == ProjectStatus.Completed || Status == ProjectStatus.Cancelled)
                {
                    throw new Abp.UI.UserFriendlyException("Não é possível adicionar tarefas a um projeto finalizado ou cancelado.");
                }
                var task = new ProjectTask(Guid.NewGuid(), this.Id, taskName, estimatedHours);
                _tasks.Add(task);
            }

            public void RemoveTask(Guid taskId)
            {
                var taskToRemove = _tasks.FirstOrDefault(t => t.Id == taskId);
                if (taskToRemove != null)
                {
                    _tasks.Remove(taskToRemove);
                    // Disparar evento de domínio se necessário
                }
            }
            // Outros métodos que gerenciam o estado do projeto e suas tarefas...
        }

        public class ProjectTask : Entity<Guid> // Entidade dentro do agregado CustomProject
        {
            public Guid ProjectId { get; protected set; } // Chave estrangeira para a raiz do agregado
            public string TaskName { get; protected set; }
            public int EstimatedHours { get; protected set; }
            public bool IsCompleted { get; protected set; }

            protected ProjectTask() { }

            public ProjectTask(Guid id, Guid projectId, string taskName, int estimatedHours) : base(id)
            {
                ProjectId = projectId;
                TaskName = taskName;
                EstimatedHours = estimatedHours;
                IsCompleted = false;
            }

            public void CompleteTask() => IsCompleted = true;
        }
        ```
    *   **Limite do Agregado**: No exemplo acima, `CustomProject` é a raiz. Todas as operações em `ProjectTask` (adicionar, remover, completar) devem ser feitas através de métodos em `CustomProject`. O repositório só existiria para `CustomProject` (`IRepository<CustomProject, Guid>`), não para `ProjectTask`.

*   **Objetos de Valor (Value Objects)**:
    *   Objetos que descrevem características de um domínio e não possuem uma identidade conceitual. São definidos por seus atributos e são imutáveis. O ABP fornece uma classe base `ValueObject`.
    *   **Exemplo EAF (Objeto de Valor Endereço)**:
        ```csharp
        // No projeto Eaf.Common.Core ou Eaf.Middleware.Core
        using Abp.Domain.Values;
        using System.Collections.Generic;

        public class Address : ValueObject // Herda de ValueObject para implementação de igualdade
        {
            public string Street { get; private set; }
            public string City { get; private set; }
            public string Region { get; private set; }
            public string PostalCode { get; private set; }
            public string Country { get; private set; }

            // Construtor para EF Core ou desserialização
            private Address() { }

            public Address(string street, string city, string region, string postalCode, string country)
            {
                Street = street;
                City = city;
                Region = region;
                PostalCode = postalCode;
                Country = country;
            }

            // ValueObject implementa GetAtomicValues para comparação estrutural
            protected override IEnumerable<object> GetAtomicValues()
            {
                yield return Street;
                yield return City;
                yield return Region;
                yield return PostalCode;
                yield return Country;
            }
        }

        // Uso em uma Entidade EAF:
        // public class EafCustomer : Entity<int>
        // {
        //     public string Name { get; set; }
        //     public Address BillingAddress { get; set; } // Propriedade do tipo Value Object
        //     public Address ShippingAddress { get; set; }
        // }
        ```
    *   O EF Core pode ser configurado para tratar `Address` como um "owned entity type" ou "complex type".

*   **Repositórios (Repositories)**:
    *   Abstrações que encapsulam a lógica de acesso a dados para agregados. O ABP fornece `IRepository<TEntity, TPrimaryKey>` e implementações base. O EAF usa esses repositórios extensivamente.
    *   **Exemplo EAF (Uso em um Serviço de Aplicação)**:
        ```csharp
        // No projeto Eaf.CustomProjects.Application
        using Abp.Application.Services;
        using Abp.Domain.Repositories;
        using System;
        using System.Threading.Tasks;

        public class CustomProjectAppService : ApplicationService, ICustomProjectAppService
        {
            private readonly IRepository<CustomProject, Guid> _customProjectRepository;

            public CustomProjectAppService(IRepository<CustomProject, Guid> customProjectRepository)
            {
                _customProjectRepository = customProjectRepository;
            }

            public async Task<CustomProjectDto> GetProjectDetailsAsync(Guid projectId)
            {
                var project = await _customProjectRepository.GetAsync(projectId);
                // Carregar explicitamente a coleção Tasks se não for carregada por padrão (lazy loading)
                // await _customProjectRepository.EnsureCollectionLoadedAsync(project, p => p.Tasks);
                return ObjectMapper.Map<CustomProjectDto>(project);
            }

            public async Task CreateProjectAsync(CreateCustomProjectDto input)
            {
                var project = new CustomProject(Guid.NewGuid(), input.ProjectName, input.Description, input.StartDate);
                // ... outras inicializações ...
                await _customProjectRepository.InsertAsync(project);
            }
            // ... outros métodos ...
        }
        ```
    *   O EAF pode definir repositórios customizados (e.g., `ICustomProjectRepository`) se precisar de métodos de consulta específicos não cobertos pelo `IRepository` genérico, embora muitos casos possam ser resolvidos com a especificação pattern ou LINQ diretamente sobre `IRepository`.

*   **Serviços de Domínio (Domain Services)**:
    *   Contêm lógica de domínio que não pertence naturalmente a uma única entidade ou objeto de valor. Devem ser stateless.
    *   **Exemplo EAF (Serviço de Domínio para Atribuição de Projetos)**:
        ```csharp
        // No projeto Eaf.CustomProjects.Core
        using Abp.Domain.Services;
        using Abp.Domain.Repositories;
        using System;
        using System.Threading.Tasks;
        // using Eaf.Authorization.Users; // Supondo uma entidade User

        public interface IProjectAssignmentService : IDomainService
        {
            Task AssignUserToProjectAsync(Guid projectId, long userId, string roleInProject);
        }

        public class ProjectAssignmentService : DomainService, IProjectAssignmentService
        {
            private readonly IRepository<CustomProject, Guid> _projectRepository;
            // private readonly IRepository<User, long> _userRepository; // Repositório de Usuários do ABP/EAF

            public ProjectAssignmentService(
                IRepository<CustomProject, Guid> projectRepository
                /*, IRepository<User, long> userRepository */)
            {
                _projectRepository = projectRepository;
                // _userRepository = userRepository;
            }

            public async Task AssignUserToProjectAsync(Guid projectId, long userId, string roleInProject)
            {
                var project = await _projectRepository.GetAsync(projectId);
                // var user = await _userRepository.GetAsync(userId);

                // if (project == null || user == null)
                //    throw new Abp.UI.UserFriendlyException("Projeto ou Usuário não encontrado.");

                // Lógica de negócios complexa:
                // - Verificar se o usuário pode ser atribuído (e.g., limite de usuários no projeto)
                // - Verificar se o papel é válido para o projeto
                // - Registrar a atribuição (pode envolver outra entidade, e.g., ProjectUserAssignment)
                // - Disparar um evento de domínio "UserAssignedToProjectEvent"

                // Exemplo: (simplificado)
                // project.AssignUser(user, roleInProject); // Método no agregado CustomProject
                Logger.Info($"Usuário {userId} atribuído ao projeto {projectId} com o papel {roleInProject}.");
            }
        }
        ```

*   **Serviços de Aplicação (Application Services)**:
    *   Como mencionado, eles dirigem os casos de uso. Eles não contêm lógica de negócios, mas coordenam a interação entre a UI e o domínio. No ABP, os métodos de serviços de aplicação são automaticamente cobertos por uma Unidade de Trabalho (UoW) e podem ter autorização e validação aplicadas automaticamente. (Ver exemplo de `CustomProjectAppService` acima).

*   **Unidade de Trabalho (Unit of Work - UoW)**:
    *   O ABP gerencia transações de banco de dados automaticamente para métodos de serviços de aplicação através do sistema de UoW. O EAF se beneficia diretamente dessa funcionalidade.
    *   Métodos de `ApplicationService` são UoW por padrão. Para outros cenários (e.g., dentro de um `DomainService` que é chamado por um `ApplicationService` já em UoW, ou em um `BackgroundJob`), pode-se usar `IUnitOfWorkManager` para controlar o UoW programaticamente ou com o atributo `[UnitOfWork]`.

*   **Eventos de Domínio (Domain Events)**:
    *   Mecanismo para capturar ocorrências significativas no domínio. O ABP fornece `IEventBus`.
    *   **Exemplo EAF (Evento e Manipulador)**:
        ```csharp
        // No projeto Eaf.CustomProjects.Core (ou Eaf.Core.Shared)
        using Abp.Events.Bus;
        using System;

        // 1. Definição do Evento de Domínio
        public class ProjectStartedEventData : EventData // EventData é uma classe base do ABP
        {
            public Guid ProjectId { get; }
            public string ProjectName { get; }
            public DateTime StartTime { get; }

            public ProjectStartedEventData(Guid projectId, string projectName)
            {
                ProjectId = projectId;
                ProjectName = projectName;
                StartTime = Clock.Now; // Clock do ABP para tempo testável
            }
        }

        // Disparando o evento (e.g., dentro do método StartProject da entidade CustomProject ou em um AppService)
        // public void StartProject()
        // {
        //     if (Status == ProjectStatus.Planning)
        //     {
        //         Status = ProjectStatus.InProgress;
        //         EventBus.Default.Trigger(new ProjectStartedEventData(this.Id, this.ProjectName));
        //     }
        // }

        // 2. Manipulador do Evento de Domínio
        // No projeto Eaf.CustomProjects.Application ou Eaf.CustomProjects.Core
        using Abp.Events.Bus.Handlers;
        using System.Threading.Tasks;
        using Microsoft.Extensions.Logging; // Usar ILogger do .NET Core

        public class ProjectStartedNotifierHandler : IAsyncEventHandler<ProjectStartedEventData>, ITransientDependency
        {
            private readonly ILogger<ProjectStartedNotifierHandler> _logger;
            // Injetar outros serviços, como um serviço de notificação por email

            public ProjectStartedNotifierHandler(ILogger<ProjectStartedNotifierHandler> logger)
            {
                _logger = logger;
            }

            public Task HandleEventAsync(ProjectStartedEventData eventData)
            {
                _logger.LogInformation($"Projeto '{eventData.ProjectName}' (ID: {eventData.ProjectId}) iniciado em {eventData.StartTime}. Notificando stakeholders...");
                // Lógica para enviar notificações, etc.
                // Ex: await _notificationService.NotifyProjectStartedAsync(eventData.ProjectId);
                return Task.CompletedTask;
            }
        }
        ```
    *   O manipulador de eventos (`ProjectStartedNotifierHandler`) será registrado automaticamente pelo ABP se estiver no mesmo assembly que o evento ou se o assembly for escaneado.

<!-- Descrições de Diagramas para DDD no EAF -->

### Diagrama 1: Componentes DDD em um Módulo EAF

**Descrição do Diagrama:** Este diagrama deve ilustrar a arquitetura em camadas de um módulo de exemplo do EAF (e.g., "Módulo de Gerenciamento de Projetos Customizados") e como os componentes DDD se encaixam.

*   **Título:** Arquitetura DDD no Módulo "Projetos Customizados" do EAF
*   **Camadas Horizontais:**
    *   **Apresentação (Eaf.Web.Mvc / Eaf.Web.Host / SPA Cliente)**: Contém Controllers, Views, ViewModels/DTOs de UI. Interage com a Camada de Aplicação.
    *   **Aplicação (Eaf.CustomProjects.Application)**: Contém `CustomProjectAppService` (usando DTOs como `CustomProjectDto`, `CreateCustomProjectDto`). Orquestra chamadas para a Camada de Domínio.
    *   **Domínio (Eaf.CustomProjects.Core)**:
        *   **Agregados**: `CustomProject` (Aggregate Root) contendo a entidade `ProjectTask` e o Value Object `Address` (se aplicável a um projeto).
        *   **Entidades**: `CustomProject`, `ProjectTask`.
        *   **Objetos de Valor**: `Address` (se usado no contexto do projeto).
        *   **Serviços de Domínio**: `IProjectAssignmentService` e sua implementação `ProjectAssignmentService`.
        *   **Interfaces de Repositório**: `IRepository<CustomProject, Guid>`.
        *   **Eventos de Domínio**: `ProjectStartedEventData`.
    *   **Infraestrutura (Eaf.EntityFrameworkCore / Eaf.CustomProjects.Infrastructure)**:
        *   Implementações de Repositório: `CustomProjectRepository` (se customizado, caso contrário, a implementação genérica do ABP).
        *   Configuração do EF Core: Mapeamento de `CustomProject`, `ProjectTask`, `Address` para o banco de dados.
        *   Outros serviços de infraestrutura (e.g., clientes para serviços externos, se o domínio precisar deles via abstrações).
*   **Interações Chave:**
    *   Setas mostrando o fluxo de dependência (geralmente para baixo, das camadas superiores para as inferiores).
    *   A Camada de Aplicação usa interfaces da Camada de Domínio (Repositorios, Serviços de Domínio).
    *   A Camada de Infraestrutura implementa interfaces da Camada de Domínio.
    *   Eventos de Domínio podem ser publicados na Camada de Domínio (ou Aplicação) e manipulados por handlers em qualquer camada apropriada (geralmente Aplicação ou Domínio).

---

### Diagrama 2: Sequência de Operação "Adicionar Tarefa a Projeto"

**Descrição do Diagrama:** Este diagrama de sequência deve mostrar as interações entre os componentes DDD durante a execução de um caso de uso específico, como "Adicionar Tarefa a um Projeto Customizado".

*   **Título:** Fluxo da Operação "Adicionar Tarefa ao Projeto"
*   **Participantes (Lifelines):**
    *   `External Actor` (e.g., Usuário via UI)
    *   `CustomProjectController` (Camada de Apresentação/API)
    *   `CustomProjectAppService` (Camada de Aplicação)
    *   `IRepository<CustomProject, Guid>` (Abstração do Repositório)
    *   `CustomProject` (Aggregate Root)
    *   `ProjectTask` (Entidade dentro do Agregado)
    *   `IEventBus` (para Eventos de Domínio, opcional no fluxo direto mas bom de mostrar)
*   **Fluxo de Mensagens:**
    1.  `External Actor` -> `CustomProjectController`: `POST /api/projects/{projectId}/tasks` com dados da tarefa (e.g., `AddTaskDto`).
    2.  `CustomProjectController` -> `CustomProjectAppService`: Chama `AddTaskToProjectAsync(projectId, addTaskDto)`.
    3.  `CustomProjectAppService` -> `IRepository<CustomProject, Guid>`: `GetAsync(projectId)` para carregar o agregado `CustomProject`.
    4.  `IRepository<CustomProject, Guid>` -> `CustomProjectAppService`: Retorna a instância de `CustomProject`.
    5.  `CustomProjectAppService` -> `CustomProject` (instância): Chama o método `AddTask(taskName, estimatedHours)`.
    6.  `CustomProject` -> `ProjectTask` (nova instância): Cria uma nova `ProjectTask`.
    7.  `CustomProject` (instância): Adiciona a nova `ProjectTask` à sua coleção interna `_tasks`.
        *   (Opcional) `CustomProject` -> `IEventBus`: `TriggerAsync(new TaskAddedToProjectEvent(...))` se houver um evento de domínio para isso.
    8.  `CustomProjectAppService` -> `IRepository<CustomProject, Guid>`: `UpdateAsync(project)` (ou o Unit of Work do ABP lida com isso automaticamente ao final do método do AppService).
    9.  `CustomProjectAppService` -> `CustomProjectController`: Retorna resultado/DTO.
    10. `CustomProjectController` -> `External Actor`: Retorna HTTP Response (e.g., 200 OK com DTO da tarefa ou 201 Created).
*   **Notas no Diagrama:**
    *   Destacar o limite transacional (Unit of Work) em torno do método do `CustomProjectAppService`.
    *   Mostrar que a lógica de validação e criação da `ProjectTask` é encapsulada dentro do agregado `CustomProject`.

<!-- Fim das Descrições de Diagramas -->

## Linguagem Ubíqua (Ubiquitous Language)

Um aspecto central do DDD é o desenvolvimento de uma Linguagem Ubíqua – um vocabulário comum compartilhado por desenvolvedores, especialistas do domínio e outros stakeholders. O EAF se esforça para usar termos consistentes do domínio no código, na documentação e nas discussões, para minimizar ambiguidades e garantir que o software reflita com precisão o modelo de negócios.

## Referência ao ABP

Muitos dos conceitos fundamentais de DDD são implementados e explicados em detalhes na [documentação do ASP.NET Boilerplate](https://aspnetboilerplate.com/Pages/Documents/Domain-Driven-Design). Recomenda-se a consulta a esta documentação para um entendimento mais aprofundado das primitivas de DDD fornecidas pelo framework base do EAF.

Ao aplicar os princípios do DDD, o EAF visa criar um sistema que não apenas atenda aos requisitos funcionais, mas que também seja resiliente, adaptável e compreensível para todos os envolvidos em seu ciclo de vida.
