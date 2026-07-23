# Multi-Tenancy no EAF (Enterprise Application Foundation)

## 1. Introdução à Multi-Tenancy no EAF

Multi-tenancy é um princípio de arquitetura de software onde uma única instância de uma aplicação serve múltiplos clientes (tenants). Cada tenant tem uma visão isolada da aplicação, com seus próprios dados, configurações e, possivelmente, customizações de funcionalidades, enquanto compartilha a mesma infraestrutura de aplicação subjacente.

O EAF (Enterprise Application Foundation) é uma plataforma open source que implementa uma arquitetura multi-tenant robusta, aproveitando as extensas funcionalidades fornecidas pelo **ASP.NET Boilerplate Zero (`AbpZeroCoreModule` ou um módulo similar)**. O ABP Zero oferece um framework completo para gerenciamento de tenants, isolamento de dados, e funcionalidades específicas por tenant.

Esta abordagem oferece benefícios como:
*   **Custo-Eficiência**: Compartilhamento de recursos de infraestrutura e manutenção.
*   **Manutenção Simplificada**: Atualizações e manutenções são aplicadas a uma única base de código.
*   **Escalabilidade**: A arquitetura pode ser escalada para suportar muitos tenants.

## 2. Conceitos Centrais de Multi-Tenancy no EAF/ABP

### 2.1. Entidade Tenant

No coração do sistema multi-tenant do ABP Zero está a entidade `Tenant` (geralmente `AbpTenant` ou uma classe derivada como `EafTenant` se o EAF a estender).

*   **Propriedades Chave**:
    *   `Id` (int): Identificador único do tenant.
    *   `TenancyName` (string): Um nome único, curto e geralmente usado em URLs ou subdomínios para identificar o tenant (e.g., "acmecorp").
    *   `Name` (string): Nome completo/display do tenant (e.g., "Acme Corporation").
    *   `IsActive` (bool): Indica se o tenant está ativo e pode usar a aplicação.
    *   `ConnectionString` (string, opcional): Se o tenant tiver seu próprio banco de dados, esta propriedade armazena a string de conexão.
    *   `EditionId` (int, opcional): Se a aplicação usar edições para gerenciar features, isso vincula o tenant a uma edição.

### 2.2. Gerenciamento de Tenants

*   **Criação e Manutenção**:
    *   O ABP Zero normalmente fornece uma UI administrativa (host-side) para criar, configurar e gerenciar tenants.
    *   Programaticamente, o `AbpTenantManager` (ou um `EafTenantManager` derivado) é usado para operações CRUD em tenants.
    ```csharp
    // Exemplo de criação de tenant programaticamente
    // using Abp.Application.Services;
    // using Abp.Domain.Uow;
    // using Abp.MultiTenancy; // Para AbpTenantManager, Tenant
    // using System.Threading.Tasks;
    //
    // public class MyTenantSetupService : ApplicationService // Ou ITransientDependency
    // {
    //    private readonly AbpTenantManager<Tenant, User> _tenantManager; // Supondo Tenant e User do ABP Zero
    //    private readonly IUnitOfWorkManager _unitOfWorkManager;
    //
    //    public MyTenantSetupService(AbpTenantManager<Tenant, User> tenantManager, IUnitOfWorkManager unitOfWorkManager)
    //    {
    //        _tenantManager = tenantManager;
    //        _unitOfWorkManager = unitOfWorkManager;
    //    }
    //
    //    [UnitOfWork]
    //    public virtual async Task CreateTenantAsync(string tenancyName, string name) // adminEmailAddress omitido para simplicidade
    //    {
    //        var tenant = new Tenant(tenancyName, name);
    //        // Configurar connection string se for DB-per-tenant
    //        // tenant.ConnectionString = "...";
    //        await _tenantManager.CreateAsync(tenant);
    //
    //        // Criar usuário admin para o novo tenant (requer UoW no contexto do novo tenant)
    //        // Esta lógica geralmente está no TenantManager ou é mais complexa envolvendo UserStore etc.
    //        // using (_unitOfWorkManager.Current.SetTenantId(tenant.Id))
    //        // {
    //        //     // ... criar usuário admin, papéis padrão para o tenant ...
    //        // }
    //    }
    // }
    ```

### 2.3. Contexto Host vs. Tenant

A aplicação opera em um de dois contextos:

*   **Contexto Host**: Operações que não pertencem a um tenant específico, como gerenciamento de tenants, configurações globais do sistema. Neste contexto, `IAbpSession.TenantId` é `null`.
*   **Contexto Tenant**: Operações realizadas por ou para um tenant específico. Neste contexto, `IAbpSession.TenantId` terá o `Id` do tenant atual.

O `IAbpSession` é crucial para determinar o contexto atual e é usado extensivamente pelo framework para filtragem de dados, verificação de features/settings, etc.

## 3. Estratégias de Resolução de Tenant

Para que a aplicação saiba qual tenant está fazendo uma requisição, o `TenantId` precisa ser resolvido. O ABP fornece um sistema flexível para isso:

*   **`ITenantResolveContributor`**: Interfaces que podem ser implementadas para adicionar lógica de resolução de tenant. O ABP vem com contribuidores padrão:
    *   **Domain Name / Subdomain**: Resolve o tenant com base no nome de domínio ou subdomínio da requisição (e.g., `acmecorp.eaf-app.com`). Requer configuração de DNS e mapeamento no EAF.
    *   **HTTP Header**: Resolve o tenant com base em um valor de header HTTP (e.g., `Abp.TenantId` ou `X-Tenant-Id`).
    *   **Query String**: Resolve o tenant com base em um parâmetro na query string (e.g., `?tenantId=123`).
    *   **Cookie**: Resolve o tenant com base em um valor armazenado em um cookie.
*   **Cadeia de Resolução**: Esses contribuidores são executados em uma cadeia até que um deles resolva o `TenantId` com sucesso.
*   **Customização no EAF**: O EAF pode adicionar seus próprios `ITenantResolveContributor` ou reordenar os existentes para se adequar a requisitos específicos de roteamento ou identificação de tenant. A configuração é feita no método `PreInitialize` de um módulo, e.g.:
    ```csharp
    // Configuration.MultiTenancy.Resolvers.InsertBefore<DomainTenantResolveContributor>(new MyCustomTenantResolveContributor());
    ```

## 4. Isolamento de Dados

Garantir que cada tenant acesse apenas seus próprios dados é fundamental.

### 4.1. Interfaces `IMayHaveTenant` e `IMustHaveTenant`

Estas interfaces são usadas para marcar entidades:

*   **`IMayHaveTenant`**: Para entidades que podem pertencer a um tenant ou ser de nível host (compartilhadas ou visíveis para todos os tenants, ou específicas do host). Estas entidades terão uma propriedade `int? TenantId`.
    *   Se `TenantId` for `null`, a entidade pertence ao host.
    *   Se `TenantId` tiver um valor, a entidade pertence a esse tenant específico.
*   **`IMustHaveTenant`**: Para entidades que *sempre* devem pertencer a um tenant. Estas entidades terão uma propriedade `int TenantId` (não-nulável). Não podem existir dados de nível host para estas entidades.

**Exemplos C# de Entidades EAF**:
```csharp
// No projeto Core de um módulo EAF
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing; // Para FullAuditedEntity
using System; // Para Guid

// Entidade que SEMPRE pertence a um tenant
public class EafTenantSpecificProduct : FullAuditedEntity<Guid>, IMustHaveTenant
{
    public int TenantId { get; set; } // Propriedade IMustHaveTenant
    public string ProductName { get; set; }
    // ... outras propriedades ...

    protected EafTenantSpecificProduct() { } // Para EF Core
    public EafTenantSpecificProduct(Guid id, string productName) : base(id) { ProductName = productName; }
}

// Entidade que PODE pertencer a um tenant ou ser de nível host
public class EafSharedCategory : FullAuditedEntity<Guid>, IMayHaveTenant
{
    public int? TenantId { get; set; } // Propriedade IMayHaveTenant (nulável)
    public string CategoryName { get; set; }
    // ... outras propriedades ...

    protected EafSharedCategory() { } // Para EF Core
    public EafSharedCategory(Guid id, string categoryName) : base(id) { CategoryName = categoryName; }
}
```

### 4.2. Filtragem Automática de Dados

O ABP aplica automaticamente filtros de `TenantId` nas consultas ao banco de dados para entidades que implementam `IMayHaveTenant` ou `IMustHaveTenant`. Isso garante que:

*   Ao operar no contexto de um tenant, apenas os dados desse tenant (e dados do host para `IMayHaveTenant` que sejam `TenantId = null`) sejam recuperados.
*   Ao operar no contexto host, apenas dados do host (`TenantId = null`) sejam recuperados para entidades `IMayHaveTenant`.

Este filtro é aplicado no nível do Unit of Work (UoW).

### 4.3. Connection Strings por Tenant (Banco de Dados por Tenant)

Por padrão, todos os tenants compartilham o mesmo banco de dados, e o isolamento é feito pela coluna `TenantId`. No entanto, o ABP Zero permite que cada tenant tenha seu próprio banco de dados.

*   **Configuração**: A string de conexão para o banco de dados do tenant é armazenada na propriedade `ConnectionString` da entidade `AbpTenant`.
*   **Comportamento**: Se um tenant tiver uma string de conexão definida, o ABP usará esse banco de dados para todas as operações desse tenant. Caso contrário, usará o banco de dados padrão (host).
*   **Implicações**: Migrações de banco de dados precisam ser aplicadas a cada banco de dados de tenant. O gerenciamento de múltiplos bancos de dados adiciona complexidade operacional.

### 4.4. Trocando o Contexto do Tenant

Em cenários avançados (e.g., um job do host que precisa processar dados para um tenant específico), pode ser necessário trocar programaticamente o contexto do tenant.

*   **`IUnitOfWorkManager.Current.SetTenantId(int? tenantId)`**: Define o `TenantId` para o UoW atual.
    ```csharp
    // Em um serviço do Host
    using Abp.Domain.Uow;
    using Abp.Domain.Repositories;
    using System.Threading.Tasks;
    using System; // Para Guid e int?
    // ...
    public class HostDataProcessorService : ITransientDependency
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<EafTenantSpecificProduct, Guid> _productRepository;

        public HostDataProcessorService(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<EafTenantSpecificProduct, Guid> productRepository)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _productRepository = productRepository;
        }

        public async Task ProcessProductForTenantAsync(int targetTenantId, Guid productId)
        {
           using (var uow = _unitOfWorkManager.Begin()) // Inicia um UoW
           {
               using (_unitOfWorkManager.Current.SetTenantId(targetTenantId)) // Define o contexto do tenant
               {
                   // Agora, todas as operações de repositório serão para targetTenantId
                   var product = await _productRepository.FirstOrDefaultAsync(productId);
                   if (product != null) { /* ... processar produto ... */ }
                   await uow.CompleteAsync(); // Completa o UoW
               }
           }
        }
    }
    ```
*   O bloco `using (_unitOfWorkManager.Current.SetTenantId(tenantId))` garante que o `TenantId` anterior seja restaurado ao sair do bloco, evitando vazamento de contexto.

## 5. Features e Configurações por Tenant

O EAF/ABP permite que features e configurações da aplicação sejam específicas por tenant.

*   **Features (`IFeatureChecker`)**:
    *   Features podem ser habilitadas ou desabilitadas para cada tenant (ou para edições às quais os tenants estão inscritos).
    *   **Exemplo C#**:
        ```csharp
        // using Abp.Application.Features;
        // ...
        // if (await FeatureChecker.IsEnabledAsync("Eaf.AdvancedReportingFeature"))
        // {
        //     // Executar lógica da feature avançada de relatórios para o tenant atual
        // }
        ```
*   **Configurações (`ISettingManager`)**:
    *   Permite que tenants tenham valores diferentes para configurações da aplicação.
    *   **Exemplo C#**:
        ```csharp
        // using Abp.Configuration;
        // using Abp.Runtime.Session; // Para IAbpSession
        // ...
        // // Dentro de um método de serviço onde IAbpSession e ISettingManager são injetados
        // var emailSignature = await SettingManager.GetSettingValueForTenantAsync(
        //     "Eaf.Email.DefaultSignature", AbpSession.GetTenantId() // GetTenantId() é mais seguro que .TenantId direto
        // );
        // // Usar emailSignature específico do tenant
        ```

## 6. Considerações de Multi-Tenancy Específicas do EAF

*   **Autenticação Externa por Tenant**:
    *   O EAF fornece extensões para permitir que cada tenant configure seus próprios provedores de identidade externos (e.g., Azure AD, SAML2, OpenID Connect).
    *   Consulte `docs/architecture/eaf-extensions.md` para detalhes sobre `TenantBasedOpenIdConnectExternalLoginInfoProvider` e similares.
*   **Onboarding e Gerenciamento de Tenants**:
    *   O EAF pode estender a UI padrão do ABP Zero para gerenciamento de tenants com fluxos de trabalho de onboarding customizados, configurações específicas do EAF por tenant, ou branding.
*   **Jobs e Processos Tenant-Aware**:
    *   Background jobs no EAF podem precisar ser projetados para operar em um contexto de tenant específico ou iterar sobre múltiplos tenants. Isso pode envolver o uso de `IUnitOfWorkManager.Current.SetTenantId()` dentro do job, geralmente passando o `tenantId` como argumento para o job.

## 7. Troubleshooting de Problemas Comuns de Multi-Tenancy

*   **Problemas de Resolução de Tenant**:
    *   O `TenantId` não está sendo identificado corretamente a partir da URL, header, ou cookie.
    *   Verifique a configuração dos `ITenantResolveContributor` e se os dados esperados (e.g., nome do subdomínio, valor do header) estão sendo enviados corretamente.
*   **Vazamento de Dados ou Filtragem Incorreta**:
    *   **Causa Comum**: Entidades não implementando `IMayHaveTenant`/`IMustHaveTenant` corretamente, ou repositórios customizados que não respeitam os filtros automáticos de tenant.
    *   **Verificar**: Implementação das interfaces nas entidades, consultas manuais que podem estar bypassando os filtros. Assegure-se que `SetTenantId` está sendo usado corretamente se estiver trocando de contexto.
*   **Problemas com Configurações ou Features Específicas do Tenant**:
    *   Verifique se os valores padrão (nível host ou edição) não estão sobrescrevendo ou sendo usados incorretamente em vez dos valores específicos do tenant.
    *   Confirme se o `TenantId` correto está ativo no momento da verificação da feature ou da obtenção da configuração.
*   **Funcionalidades "Host-Only" ou "Tenant-Only"**:
    *   Assegure-se de que funcionalidades destinadas apenas ao host não sejam acessíveis por tenants, e vice-versa. Use `AbpAuthorize` com verificações de `AbpSession.TenantId == null` (para host) ou `AbpSession.TenantId != null` (para tenant) se necessário, ou defina permissões específicas para host/tenant.
    *   Lembre-se que `[AbpAllowAnonymous]` bypassa a necessidade de um `TenantId` resolvido para aquela ação específica, o que pode ser útil para páginas de login de tenant.
*   **Cache**: Se o caching não for tenant-aware (e.g., chaves de cache não incluem `TenantId`), dados de um tenant podem vazar para outro. O `ICacheManager` do ABP geralmente lida com isso prefixando chaves de cache com o `TenantId`, mas caches customizados ou uso direto de `IDistributedCache` precisam ser implementados com cuidado.

A arquitetura multi-tenant do EAF, herdada e estendida do ABP Zero, é poderosa e flexível, mas requer uma compreensão cuidadosa de seus conceitos para garantir o isolamento correto e o comportamento esperado em todos os contextos.
