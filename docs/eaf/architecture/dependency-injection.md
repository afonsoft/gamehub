# Injeção de Dependência (DI) no EAF

A Injeção de Dependência (DI) é um padrão de design fundamental utilizado extensivamente no Enterprise Application Framework (EAF) e em seu framework base, o ASP.NET Boilerplate (ABP). A DI permite escrever código mais desacoplado, modular, testável e de fácil manutenção. Em vez de os componentes criarem suas próprias dependências, essas dependências são "injetadas" de fora, geralmente por um contêiner de DI.

## DI no ASP.NET Boilerplate (ABP)

O ASP.NET Boilerplate vem com um sistema de DI robusto e integrado:

*   **Contêiner de DI Padrão - Castle Windsor**: Por padrão, o ABP utiliza o [Castle Windsor](https://github.com/castleproject/Windsor) como seu contêiner de Injeção de Dependência. O Castle Windsor é um contêiner de DI maduro e poderoso para .NET.
*   **Convenção sobre Configuração**: O ABP favorece a convenção sobre configuração para o registro de dependências. Isso significa que muitas classes são registradas automaticamente no contêiner de DI com base em interfaces que implementam ou classes base das quais herdam:
    *   **`ITransientDependency`**: Classes que implementam esta interface são registradas com um ciclo de vida transitório (uma nova instância é criada cada vez que é solicitada).
    *   **`ISingletonDependency`**: Classes que implementam esta interface são registradas como singletons (uma única instância é criada e compartilhada durante todo o ciclo de vida da aplicação).
    *   **`IPerWebRequestDependency`** (em aplicações web tradicionais ABP) / **`IScopedDependency`** (com a integração ASP.NET Core): Classes registradas com um ciclo de vida por requisição web ou por escopo definido.
*   **Registro Automático de Tipos Comuns**: Componentes padrão do ABP, como Serviços de Aplicação (Application Services), Serviços de Domínio (Domain Services) e Repositórios (Repositories), são automaticamente descobertos e registrados no contêiner de DI sem a necessidade de registro manual explícito.

## Registro de Dependências no EAF

O EAF segue as convenções do ABP para o registro de suas próprias dependências. A principal forma de registrar componentes de um módulo específico é através do método `IocManager.RegisterAssemblyByConvention` dentro da classe de módulo do ABP:

```csharp
// Exemplo dentro de uma classe de Módulo do EAF (ex: EafMyModule.cs)
using Abp.Modules;
using Abp.Reflection.Extensions;
// ... outros usings

public class EafMyModule : AbpModule
{
    public override void Initialize()
    {
        // Registra todos os serviços neste assembly que seguem as convenções do ABP
        // (e.g., implementam ITransientDependency, ISingletonDependency)
        IocManager.RegisterAssemblyByConvention(typeof(EafMyModule).GetAssembly());

        // Outras inicializações específicas do módulo podem vir aqui
    }
}
```

Esta linha instrui o `IocManager` (o resolvedor de dependências do ABP) a varrer o assembly onde `EafMyModule` está definido e registrar todas as classes que seguem as convenções de DI (implementando `ITransientDependency`, etc.).

### Registros Específicos e Avançados no EAF

Embora o registro por convenção cubra a maioria dos cenários, o EAF pode precisar de registros mais explícitos para casos específicos:

*   **Substituição de Serviços**: Conforme mencionado, `Configuration.ReplaceService` no método `PreInitialize` de um módulo é a forma padrão do ABP para substituir serviços existentes. O EAF utiliza isso para customizar comportamentos do framework base.
    ```csharp
    // Em um módulo EAF, por exemplo, EafCustomCoreModule.cs
    public override void PreInitialize()
    {
        // Substitui o serviço de notificação padrão do ABP por uma implementação EAF
        Configuration.ReplaceService<INotificationPublisher, EafNotificationPublisher>(DependencyLifeStyle.Transient);
    }
    ```

*   **Registro Explícito com `IocManager`**: Para classes que não seguem as convenções padrão ou requerem configurações especiais (como nomeadas ou com dependências complexas).
    ```csharp
    // No método Initialize de um módulo EAF
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(EafMyModule).GetAssembly());

        // Exemplo: Registrar uma factory manualmente
        IocManager.Register<IMyEafWidgetFactory, MyEafWidgetFactory>(DependencyLifeStyle.Singleton);

        // Exemplo: Registrar um serviço com um nome específico (menos comum)
        // IocManager.IocContainer.Register(
        //    Component.For<IMyNamedService>().ImplementedBy<MyNamedServiceImpl>().Named("MySpecialInstance")
        // );
    }
    ```
    *Nesses casos, `IMyEafWidgetFactory` e `MyEafWidgetFactory` seriam classes definidas pelo EAF.*

*   **Registro Condicional**: O EAF pode registrar serviços com base em configurações.
    ```csharp
    // No método Initialize de um módulo EAF
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(EafMyModule).GetAssembly());

        if (Configuration.Modules.EafMyModule().EnableAdvancedFeature)
        {
            IocManager.Register<IAdvancedFeatureService, AdvancedFeatureServiceImpl>(DependencyLifeStyle.Transient);
        }
        else
        {
            IocManager.Register<IAdvancedFeatureService, BasicFeatureServiceImpl>(DependencyLifeStyle.Transient);
        }
    }
    ```

### Convenções de Registro por Módulo/Camada no EAF

O EAF, seguindo o ABP, organiza o código em módulos e camadas. A prática padrão é que cada módulo (especialmente módulos de camada como `Eaf.Application`, `Eaf.Core`, `Eaf.Infrastructure`) chame `IocManager.RegisterAssemblyByConvention(typeof(MySpecificModule).GetAssembly());` em seu método `Initialize()`. Isso garante que os serviços definidos em cada assembly de módulo sejam descobertos e registrados, promovendo a modularidade.

## Injeção de Dependências

A forma predominante de receber dependências no EAF (e ABP) é através da **injeção pelo construtor**:

```csharp
public class MyEafAppService : ApplicationService
{
    private readonly IRepository<MyEntity> _myEntityRepository;
    private readonly MyDomainService _myDomainService;

    // Dependências são injetadas no construtor
    public MyEafAppService(
        IRepository<MyEntity> myEntityRepository,
        MyDomainService myDomainService)
    {
        _myEntityRepository = myEntityRepository;
        _myDomainService = myDomainService;
    }

    public void DoSomething()
    {
        // Usa as dependências injetadas
        var entity = _myEntityRepository.Get(1);
        _myDomainService.ProcessEntity(entity);
    }
}
```

A **injeção de propriedade pública** (Property Injection) também é suportada pelo Castle Windsor e ABP. Embora a injeção pelo construtor seja preferida para a maioria dos cenários devido à sua clareza em expressar dependências obrigatórias, a injeção de propriedade pode ser útil em certas classes base do framework ou em componentes onde o construtor não pode ser facilmente modificado (e.g., atributos, algumas classes de UI).

No ABP e, por extensão, no EAF, propriedades públicas e virtuais podem ser automaticamente preenchidas pelo contêiner de DI. É comum ver isso em classes base como `ApplicationService`, `DomainService`, e `AbpController`, onde propriedades como `Logger`, `LocalizationManager`, `ObjectMapper`, `PermissionChecker`, `SettingManager`, `AbpSession`, e `EventBus` são injetadas.

```csharp
// Exemplo de uma classe base EAF ou um serviço que utiliza injeção de propriedade
public abstract class EafCustomServiceBase : ITransientDependency // Ou outra interface de ciclo de vida
{
    // Propriedade pública e virtual para permitir a injeção pelo Castle Windsor
    public virtual ICustomEafUtilityService CustomUtility { get; set; }

    // Exemplo de uma propriedade comum do ABP, que seria injetada da mesma forma
    public IAbpSession AbpSession { get; set; }

    protected EafCustomServiceBase()
    {
        // É uma boa prática inicializar propriedades injetadas com implementações nulas (Null Object Pattern)
        // para evitar NullReferenceException se a DI não as preencher (embora o Windsor geralmente o faça).
        // O ABP faz isso para suas propriedades base (e.g., Logger = NullLogger.Instance).
        CustomUtility = NullCustomEafUtilityService.Instance; // Supondo uma implementação Null Object
        AbpSession = NullAbpSession.Instance; // NullAbpSession é fornecido pelo ABP
    }

    protected void PerformOperation()
    {
        // CustomUtility e AbpSession podem ser usados aqui, pois serão preenchidos pela DI.
        if (CustomUtility.IsFeatureEnabledForTenant(AbpSession.TenantId))
        {
            // ...
        }
    }
}

// Interface e implementação do serviço de utilidade (exemplo)
public interface ICustomEafUtilityService : ITransientDependency
{
    bool IsFeatureEnabledForTenant(int? tenantId);
}
public class CustomEafUtilityService : ICustomEafUtilityService
{
    public bool IsFeatureEnabledForTenant(int? tenantId) { /* ... lógica ... */ return true; }
}
public class NullCustomEafUtilityService : ICustomEafUtilityService // Exemplo de Null Object
{
    public static readonly NullCustomEafUtilityService Instance = new NullCustomEafUtilityService();
    public bool IsFeatureEnabledForTenant(int? tenantId) => false;
}
```
*Neste exemplo, se `EafCustomServiceBase` for resolvido pela DI, a propriedade `CustomUtility` será preenchida com uma instância de `CustomEafUtilityService`.*

### Resolução Manual de Dependências com `IocManager`

Em raras situações, pode ser necessário resolver dependências manualmente em vez de usar injeção pelo construtor ou propriedade. O ABP fornece a classe estática `IocManager.Instance` para isso. No entanto, **o uso excessivo da resolução manual de serviços deve ser evitado**, pois pode levar a um acoplamento mais forte com o contêiner de DI e tornar o código mais difícil de testar e entender (Service Locator anti-pattern).

**Cenários Justificáveis (Raros)**:
*   Dentro de classes estáticas ou métodos estáticos onde a DI não está disponível.
*   Em alguns pontos de extensão de frameworks de terceiros onde a injeção não é suportada nativamente.
*   Ao integrar com código legado que não foi projetado com DI em mente.

**Exemplo de Uso**:
```csharp
// Em uma classe onde a injeção não é facilmente aplicável
public static class EafLegacyHelper
{
    public static void PerformSomeAction(int itemId)
    {
        // Resolução manual de um serviço EAF
        var itemProcessor = IocManager.Instance.Resolve<IItemProcessingService>();

        try
        {
            itemProcessor.ProcessItem(itemId);
        }
        finally
        {
            // Importante: Liberar componentes resolvidos manualmente, especialmente se forem transitórios ou com escopo.
            // Se o componente resolvido for ISingletonDependency, a liberação não é estritamente necessária,
            // mas é uma boa prática para consistência se o ciclo de vida não for conhecido.
            IocManager.Instance.Release(itemProcessor);
        }
    }
}

// Interface e implementação do serviço de exemplo
public interface IItemProcessingService : ITransientDependency
{
    void ProcessItem(int itemId);
}

public class ItemProcessingService : IItemProcessingService
{
    public void ProcessItem(int itemId) { /* ... lógica de processamento ... */ }
}
```
**Precauções ao Usar `IocManager.Resolve`**:
1.  **Ciclo de Vida**: Esteja ciente do ciclo de vida do serviço que você está resolvendo. Se for um serviço `ITransientDependency` ou `IScopedDependency` resolvido em um contexto singleton ou de longa duração, você é responsável por liberá-lo usando `IocManager.Instance.Release()` para evitar memory leaks.
2.  **Testabilidade**: Código que usa `IocManager.Resolve` é mais difícil de testar isoladamente, pois depende do estado global do `IocManager`.
3.  **Acoplamento**: Cria uma dependência direta do seu código com o `IocManager` e o contêiner de DI.

## Principais Interfaces e Convenções de Registro

*   **`ITransientDependency`**: Para serviços que devem ser criados novos a cada vez que são solicitados.
*   **`IScopedDependency`**: (Recomendado em aplicações ASP.NET Core) Para serviços cujo ciclo de vida está atrelado a um escopo, tipicamente o escopo de uma requisição web. No ABP mais antigo, `IPerWebRequestDependency` tinha um papel similar.
*   **`ISingletonDependency`**: Para serviços que devem ter apenas uma instância durante todo o ciclo de vida da aplicação.
*   **`IocManager`**: Uma classe utilitária do ABP que fornece acesso direto ao contêiner de DI. Pode ser usada para resolver dependências manualmente (`IocManager.Resolve<IMyService>()`), mas seu uso deve ser minimizado em favor da injeção pelo construtor para manter o código mais limpo e testável.

## Escopos de DI (DI Scopes)

Compreender os escopos de DI é crucial em aplicações web:

*   **Transient**: Uma nova instância do serviço é criada cada vez que ele é solicitado do contêiner.
*   **Scoped (ou PerRequest)**: Uma única instância do serviço é criada por escopo. Em aplicações web ASP.NET Core, um escopo é geralmente criado para cada requisição HTTP. Isso significa que dentro da mesma requisição, todas as solicitações por um serviço com escopo `Scoped` receberão a mesma instância. Serviços como `DbContext` do Entity Framework são tipicamente registrados com este escopo.
*   **Singleton**: Uma única instância do serviço é criada na primeira vez que é solicitada e essa mesma instância é reutilizada em todas as solicitações subsequentes durante todo o ciclo de vida da aplicação. Adequado para serviços stateless ou que precisam manter um estado global.

O EAF utiliza esses escopos para gerenciar o ciclo de vida de seus componentes, garantindo que eles sejam utilizados da maneira mais eficiente e segura possível.

## Referência ao ABP

Para um entendimento mais aprofundado do sistema de Injeção de Dependência do ASP.NET Boilerplate, incluindo tópicos avançados como facilities do Windsor, interceptadores e gerenciamento de ciclo de vida, consulte a [documentação oficial do ABP sobre DI](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection).

A Injeção de Dependência é um pilar da arquitetura do EAF, permitindo que o framework seja flexível, extensível e robusto, seguindo as melhores práticas de design de software.
