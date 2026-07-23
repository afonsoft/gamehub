# Sistema de Módulos - ASP.NET Boilerplate

## Visão Geral

O ASP.NET Boilerplate fornece uma infraestrutura para construir módulos e compô-los para criar uma aplicação. Um módulo pode depender de outro módulo. Geralmente, um assembly é considerado como um módulo. Se você criou uma aplicação com mais de um assembly, é sugerido criar uma definição de módulo para cada assembly.

O sistema de módulos atualmente é focado no lado do servidor em vez do lado do cliente.

Para documentação oficial completa, consulte: [ASP.NET Boilerplate Module System v10.3](https://aspnetboilerplate.com/Pages/Documents/v10.3/Module-System)

## Definição de Módulo

Um módulo é definido com uma classe que deriva de `AbpModule` do pacote ABP. Digamos que estamos desenvolvendo um módulo Blog que pode ser usado em diferentes aplicações. A definição de módulo mais simples pode ser mostrada abaixo:

```csharp
public class MyBlogApplicationModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }
}
```

A classe de definição de módulo é responsável por registrar suas classes no sistema de [injeção de dependência](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) (pode ser feito convencionalmente como mostrado acima) se necessário. Ela pode configurar a aplicação e outros módulos, adicionar novos recursos à aplicação e assim por diante.

## Métodos de Ciclo de Vida

O ASP.NET Boilerplate chama alguns métodos específicos dos módulos na inicialização e encerramento da aplicação. Você pode substituir esses métodos para executar tarefas específicas.

O ASP.NET Boilerplate chama esses métodos ordenados por dependências. Se o módulo A depende do módulo B, o módulo B é inicializado antes do módulo A. A ordem exata dos métodos de inicialização: PreInitialize-B, PreInitialize-A, Initialize-B, Initialize-A, PostInitialize-B e PostInitialize-A. Isso é verdadeiro para todo o gráfico de dependências. O método Shutdown também é similar, mas em ordem reversa.

### PreInitialize

Este método é chamado primeiro quando a aplicação inicia. É o método usual para [configurar](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration) o framework e outros módulos antes que eles inicializem.

Além disso, você pode escrever código específico aqui para executar antes dos registros de injeção de dependência. Por exemplo, se você cria uma classe de [registro convencional](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection), você deve registrá-la aqui usando o método `IocManager.AddConventionalRegisterer`.

**Exemplo de uso:**

```csharp
public override void PreInitialize()
{
    // Configurar validação
    Configuration.Validation.Validators.Add<MyEntityValidator>();

    // Configurar AutoMapper
    Configuration.Modules.AbpAutoMapper().Configurators.Add(
        cfg => cfg.AddProfile<MyEntityProfile>()
    );

    // Configurar autorização
    Configuration.Authorization.Providers.Add<MyAuthorizationProvider>();

    // Configurar configurações do módulo
    Configuration.Modules.MyModule().MyProperty = "my-value";
}
```

### Initialize

É o lugar usual onde o registro de injeção de dependência deve ser feito. Geralmente é feito usando o método `IocManager.RegisterAssemblyByConvention`. Se você quer definir registro de dependência personalizado, consulte a [documentação de injeção de dependência](https://aspnetboilerplate.com/Pages/Documents/v2.0.0/Dependency-Injection).

**Exemplo de uso:**

```csharp
public override void Initialize()
{
    IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());

    // Registro personalizado
    IocManager.Register<IMyService, MyService>(DependencyLifeStyle.Transient);
}
```

### PostInitialize

Este método é chamado por último no progresso de inicialização. É seguro resolver uma dependência aqui.

**Exemplo de uso:**

```csharp
public override void PostInitialize()
{
    // Seguro resolver dependências aqui
    var myService = IocManager.Resolve<IMyService>();
    myService.Initialize();
}
```

### Shutdown

Este método é chamado quando a aplicação é encerrada.

**Exemplo de uso:**

```csharp
public override void Shutdown()
{
    // Limpar recursos
    // Fechar conexões
    // Salvar estado
}
```

## Dependências de Módulo

Um módulo pode depender de outro. É necessário declarar explicitamente as dependências usando o atributo `DependsOn` como abaixo:

```csharp
[DependsOn(typeof(MyBlogCoreModule))]
public class MyBlogApplicationModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }
}
```

Assim, declaramos ao ASP.NET Boilerplate que `MyBlogApplicationModule` depende de `MyBlogCoreModule` e o `MyBlogCoreModule` deve ser inicializado antes do `MyBlogApplicationModule`.

O ABP pode resolver dependências recursivamente começando do módulo de inicialização e inicializá-las de acordo. O módulo de inicialização é inicializado como o último módulo.

**Exemplo com múltiplas dependências:**

```csharp
[DependsOn(
    typeof(AbpAutoMapperModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(MyBlogCoreModule)
)]
public class MyBlogApplicationModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }
}
```

## Módulos PlugIn

Enquanto os módulos são investigados começando do módulo de inicialização e indo para as dependências, o ABP também pode carregar módulos dinamicamente. A classe `AbpBootstrapper` define uma propriedade `PlugInSources` que pode ser usada para adicionar fontes para carregar dinamicamente módulos plugin. Uma fonte plugin pode ser qualquer classe que implementa a interface `IPlugInSource`. A classe `PlugInFolderSource` implementa isso para obter módulos plugin de assemblies localizados em uma pasta.

### ASP.NET Core

O módulo ABP ASP.NET Core define opções no método de extensão `AddAbp` para adicionar fontes plugin na classe Startup:

```csharp
services.AddAbp<MyStartupModule>(options =>
{
    options.PlugInSources.Add(new FolderPlugInSource(@"C:\MyPlugIns"));
});
```

Podemos usar o método de extensão `AddFolder` para uma sintaxe mais simples:

```csharp
services.AddAbp<MyStartupModule>(options =>
{
    options.PlugInSources.AddFolder(@"C:\MyPlugIns");
});
```

Consulte o [documentação ASP.NET Core](https://aspnetboilerplate.com/Pages/Documents/v2.0.0/AspNet-Core) para mais informações sobre a classe Startup.

### ASP.NET MVC, Web API

Para aplicações clássicas ASP.NET MVC, podemos adicionar pastas plugin substituindo `Application_Start` em global.asax como mostrado abaixo:

```csharp
public class MvcApplication : AbpWebApplication<MyStartupModule>
{
    protected override void Application_Start(object sender, EventArgs e)
    {
        AbpBootstrapper.PlugInSources.AddFolder(@"C:\MyPlugIns");
        //...
        base.Application_Start(sender, e);
    }
}
```

#### Controllers em PlugIns

Se seus módulos incluem Controllers MVC ou Web API, o ASP.NET não pode investigar seus controllers. Para superar este problema, você pode alterar o arquivo global.asax como abaixo:

```csharp
using System.Web;
using Abp.PlugIns;
using Abp.Web;
using MyDemoApp.Web;

[assembly: PreApplicationStartMethod(typeof(PreStarter), "Start")]

namespace MyDemoApp.Web
{
    public class MvcApplication : AbpWebApplication<MyStartupModule>
    {
    }

    public static class PreStarter
    {
        public static void Start()
        {
            //...
            MvcApplication.AbpBootstrapper.PlugInSources.AddFolder(@"C:\MyPlugIns\");
            MvcApplication.AbpBootstrapper.PlugInSources.AddToBuildManager();
        }
    }
}
```

## Assemblies Adicionais

As implementações padrão para `IAssemblyFinder` e `ITypeFinder` (que é usado pelo ABP para investigar classes específicas na aplicação) apenas encontram assemblies de módulo e tipos nesses assemblies. Podemos substituir o método `GetAdditionalAssemblies` em nosso módulo para incluir assemblies adicionais.

**Exemplo:**

```csharp
public override void Initialize()
{
    IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());

    // Incluir assemblies adicionais
    IocManager.RegisterAssemblyByConvention(Assembly.Load("MyAdditionalAssembly"));
}
```

## Métodos de Módulo Personalizados

Seus módulos também podem ter métodos personalizados que podem ser usados por outros módulos que dependem deste módulo. Assuma que `MyModule2` depende de `MyModule1` e quer chamar um método de `MyModule1` em `PreInitialize`.

```csharp
public class MyModule1 : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }

    public void MyModuleMethod1()
    {
        // este é um método personalizado deste módulo
    }
}

[DependsOn(typeof(MyModule1))]
public class MyModule2 : AbpModule
{
    private readonly MyModule1 _myModule1;

    public MyModule2(MyModule1 myModule1)
    {
        _myModule1 = myModule1;
    }

    public override void PreInitialize()
    {
        _myModule1.MyModuleMethod1(); // Chamar o método de MyModule1
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }
}
```

Aqui, injetamos `MyModule1` no construtor de `MyModule2`, então `MyModule2` pode chamar o método personalizado de `MyModule1`. Isso é possível apenas se o `Module2` depende do `Module1`.

## Ciclo de Vida do Módulo

As classes de módulo são automaticamente registradas como singleton. Isso significa que há apenas uma instância de cada classe de módulo durante o tempo de vida da aplicação.

**Implicações:**
- As classes de módulo podem ter dependências injetadas no construtor
- As dependências são resolvidas uma vez durante a inicialização
- O estado do módulo é compartilhado durante toda a aplicação

**Exemplo:**

```csharp
public class MyModule : AbpModule
{
    private readonly ILogger _logger;

    public MyModule(ILogger logger)
    {
        _logger = logger;
    }

    public override void Initialize()
    {
        _logger.LogInfo("MyModule está sendo inicializado");
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }
}
```

## Configuração de Inicialização (Startup Configuration)

O ASP.NET Boilerplate fornece a infraestrutura e um modelo para configurar seus módulos na inicialização. A configuração é feita no método `PreInitialize` do seu módulo.

### Configuração Básica

**Exemplo de configuração completa:**

```csharp
public class SimpleTaskSystemModule : AbpModule
{
    public override void PreInitialize()
    {
        // Adicionar idiomas para a aplicação
        Configuration.Localization.Languages.Add(new LanguageInfo("en", "English", "famfamfam-flag-england", true));
        Configuration.Localization.Languages.Add(new LanguageInfo("tr", "Türkçe", "famfamfam-flag-tr"));

        // Adicionar uma fonte de localização
        Configuration.Localization.Sources.Add(
            new XmlLocalizationSource(
                "SimpleTaskSystem",
                HttpContext.Current.Server.MapPath("~/Localization/SimpleTaskSystem")
            )
        );

        // Configurar navegação/menu
        Configuration.Navigation.Providers.Add<SimpleTaskSystemNavigationProvider>();
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }
}
```

### Configuração de Localização

**Adicionar idiomas:**

```csharp
public override void PreInitialize()
{
    Configuration.Localization.Languages.Add(new LanguageInfo("en", "English", "famfamfam-flag-england", true));
    Configuration.Localization.Languages.Add(new LanguageInfo("pt", "Português", "famfamfam-flag-br"));
    Configuration.Localization.Languages.Add(new LanguageInfo("es", "Español", "famfamfam-flag-es"));
}
```

**Adicionar fontes de localização:**

```csharp
public override void PreInitialize()
{
    // Fonte XML
    Configuration.Localization.Sources.Add(
        new XmlLocalizationSource(
            "MyApp",
            Server.MapPath("~/Localization/MyApp")
        )
    );

    // Fonte JSON (se disponível)
    Configuration.Localization.Sources.Add(
        new JsonLocalizationSource(
            "MyApp",
            Server.MapPath("~/Localization/MyApp")
        )
    );
}
```

### Configuração de Navegação

**Adicionar provedores de navegação:**

```csharp
public override void PreInitialize()
{
    Configuration.Navigation.Providers.Add<MyAppNavigationProvider>();
}
```

**Exemplo de NavigationProvider:**

```csharp
public class MyAppNavigationProvider : NavigationProvider
{
    public override void SetNavigation(INavigationProviderContext context)
    {
        context.Manager.MainMenu
            .AddMenuItem(
                "HomePage",
                L("HomePage"),
                url: "",
                icon: "home"
            )
            .AddMenuItem(
                "About",
                L("About"),
                url: "home/about",
                icon: "info"
            );
    }

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, "MyApp");
    }
}
```

### Substituição de Serviços Integrados

O método `Configuration.ReplaceService` pode ser usado para substituir um serviço integrado por uma implementação personalizada.

**Exemplo de substituição:**

```csharp
public override void PreInitialize()
{
    // Substituir IAbpSession com implementação personalizada
    Configuration.ReplaceService<IAbpSession, MySession>(DependencyLifeStyle.Transient);
}
```

**Exemplo com substituição customizada:**

```csharp
public override void PreInitialize()
{
    Configuration.ReplaceService<IAbpSession>(
        (services, currentImplementation) =>
        {
            // Lógica customizada de substituição
            services.Register<IAbpSession, MySession>(DependencyLifeStyle.Transient);
        }
    );
}
```

**Nota:** O mesmo serviço pode ser substituído múltiplas vezes, especialmente em diferentes módulos. A última substituição será a válida. Os métodos `PreInitialize` são executados pela ordem de dependência dos módulos.

### Configuração de Módulo

Enquanto métodos de módulo personalizados podem ser usados para configurar módulos, sugerimos usar o sistema de [configuração de inicialização](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration) para definir e definir configuração para módulos.

**Exemplo de configuração de módulo:**

```csharp
public class MyModuleOptions
{
    public bool EnableFeature { get; set; }
    public int MaxItems { get; set; }
}

public static class MyModuleConfigurationExtensions
{
    public static void ConfigureMyModule(this AbpModuleOptions options, Action<MyModuleOptions> configure)
    {
        var moduleOptions = new MyModuleOptions();
        configure?.Invoke(moduleOptions);
        options.IocManager.Register(() => moduleOptions);
    }
}

// Uso no módulo
public override void PreInitialize()
{
    Configuration.Modules.MyModule().Configure(options =>
    {
        options.EnableFeature = true;
        options.MaxItems = 100;
    });
}
```

## Como Criar Novos Módulos no EAF

### Passo 1: Criar o Projeto do Módulo

Crie um novo projeto de biblioteca de classes (.NET Standard ou .NET):

```bash
dotnet new classlib -n Eaf.MyModule
```

### Passo 2: Adicionar Dependências do ABP

Adicione os pacotes NuGet necessários:

```bash
dotnet add Eaf.MyModule package Abp
dotnet add Eaf.MyModule package Castle.Windsor
```

### Passo 3: Criar a Classe do Módulo

Crie uma classe que herda de `AbpModule`:

```csharp
using Abp.Modules;
using Abp.Reflection.Extensions;
using Castle.Windsor.MsDependencyInjection;

namespace Eaf.MyModule
{
    [DependsOn(typeof(AbpKernelModule))]
    public class EafMyModuleModule : AbpModule
    {
        public override void PreInitialize()
        {
            // Configurações antes da inicialização
            Configuration.Settings.Providers.Add<MySettingProvider>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }

        public override void PostInitialize()
        {
            // Configurações após a inicialização
        }

        public override void Shutdown()
        {
            // Limpeza ao encerrar
        }
    }
}
```

### Passo 4: Adicionar Funcionalidades ao Módulo

Adicione entidades, serviços, DTOs, etc. ao seu módulo:

```csharp
// Entidade
public class MyEntity : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
}

// Interface de Repositório
public interface IMyEntityRepository : IRepository<MyEntity, Guid>
{
    List<MyEntity> GetActiveEntities();
}

// Implementação de Repositório
public class MyEntityRepository : EfCoreRepositoryBase<EafMyModuleDbContext, MyEntity, Guid>, IMyEntityRepository
{
    public MyEntityRepository(IDbContextProvider<EafMyModuleDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public List<MyEntity> GetActiveEntities()
    {
        return GetAll().Where(x => x.IsActive).ToList();
    }
}

// Application Service
public class MyEntityAppService : ApplicationService, IMyEntityAppService
{
    private readonly IMyEntityRepository _myEntityRepository;

    public MyEntityAppService(IMyEntityRepository myEntityRepository)
    {
        _myEntityRepository = myEntityRepository;
    }

    public async Task<MyEntityDto> GetAsync(EntityDto<Guid> input)
    {
        var entity = await _myEntityRepository.GetAsync(input.Id);
        return ObjectMapper.Map<MyEntityDto>(entity);
    }
}
```

### Passo 5: Integrar o Módulo na Aplicação

No módulo principal da aplicação, adicione a dependência ao seu novo módulo:

```csharp
[DependsOn(
    typeof(AbpAutoMapperModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(EafMyModuleModule) // Adicione seu módulo aqui
)]
public class EafProjectNameApplicationModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }
}
```

## Estrutura da Solução

### Arquitetura Lado do Servidor

Uma aplicação empresarial típica segue uma arquitetura em camadas com múltiplos projetos:

- **Core.Shared**: Contém constantes, enums e classes auxiliares usadas tanto em projetos móveis quanto web.
- **Core**: Contém classes da camada de domínio (entidades, serviços de domínio).
- **Application.Shared**: Contém interfaces de serviços de aplicação e DTOs.
- **Application**: Contém lógica de aplicação (serviços de aplicação).
- **EntityFrameworkCore**: Contém DbContext, implementações de repositório, migrações de banco de dados e conceitos específicos do Entity Framework Core.
- **Web.Host**: Serve a aplicação como API REST remota sem HTML, JS ou CSS.
- **Web.Core**: Contém classes comuns usadas por projetos MVC e Host.
- **Web.Public**: Aplicação web separada que pode ser usada para criar um site público ou landing page.
- **Migrator**: Aplicação console que executa migrações de banco de dados.
- **ConsoleApiClient**: Aplicação console simples para realizar requisições API autenticadas.
- **Tests**: Contém testes unitários e de integração.

### Arquitetura Angular

A solução Angular é projetada para ser implantada separadamente da solução backend ASP.NET Core:

- **RootModule**: Responsável por inicializar a aplicação.
- **AccountModule**: Fornece login, autenticação de dois fatores, registro, recuperação de senha, ativação de email, etc. É carregado de forma lazy.
- **AppModule**: Agrupa módulos de aplicação e fornece layout base.
  - **AdminModule**: Contém páginas como gerenciamento de usuários, roles, tenants, idiomas, configurações, etc. É carregado de forma lazy.
  - **MainModule**: Módulo principal para desenvolver sua própria aplicação. É recomendado dividir a aplicação em módulos menores.
- **Shared Modules**:
  - **app-common.module**: Módulo comum usado por módulos main e admin.
  - **common.module**: Módulo comum usado por módulos account e app.
  - **utils.module**: Módulo comum usado por todos os módulos.
  - **service-proxy.module**: Código nswag gerado automaticamente para comunicação com a API backend.

### Configuração Básica

O arquivo `appsettings.json` no projeto Web.Host contém configurações essenciais:

```json
{
  "ServerRootAddress": "https://localhost:44301/",
  "ClientRootAddress": "http://localhost:4200/",
  "CorsOrigins": "http://localhost:4200/"
}
```

- **ServerRootAddress**: URL da aplicação Web.Host
- **ClientRootAddress**: URL da aplicação Angular
- **CorsOrigins**: URLs permitidas para fazer requisições cross-origin

### Multi-Tenância

Multi-tenância é usada para construir aplicações SaaS facilmente. Com esta técnica, podemos implantar uma única aplicação para servir a múltiplos clientes. Cada Tenant terá seus próprios roles, usuários, configurações e outros dados.

**Tipos de perspectiva em aplicações multi-tenant:**
- **Host**: Gerencia tenants e o sistema.
- **Tenant**: Usa as funcionalidades reais da aplicação e paga por ela.

Para aplicações multi-tenant, URLs podem conter nome de tenant dinâmico usando `{TENANCY_NAME}` como placeholder:

```json
{
  "ServerRootAddress": "http://{TENANCY_NAME}.mydomain.com/",
  "ClientRootAddress": "http://{TENANCY_NAME}.app.mydomain.com/"
}
```

Para o CorsOrigins, você pode usar o caractere `*` para permitir todos os subdomínios:

```json
{
  "CorsOrigins": "http://*.app.mydomain.com/"
}
```

### Configuração Angular

A solução Angular contém o arquivo `src/assets/appconfig.json` com configurações fundamentais:

- **remoteServiceBaseUrl**: Endereço base das APIs do servidor (padrão: https://localhost:44301)
- **appBaseUrl**: Endereço base da aplicação cliente (padrão: http://localhost:4200)
- **localeMappings**: Configuração de localizações de bibliotecas de terceiros

## Criação de Menu Item

### Definindo um Item de Menu

No lado do cliente Angular, abra o arquivo de serviço de navegação que define os itens de menu:

```typescript
new AppMenuItem("PhoneBook", null, "flaticon-book", "/app/main/phonebook")
```

- **PhoneBook**: Nome do menu (será localizado)
- **null**: Nome da permissão (será definido posteriormente)
- **flaticon-book**: Classe de ícone arbitrário
- **/phonebook**: Rota Angular

### Localizar Nome de Exibição do Menu

Strings de localização são definidas em arquivos XML no projeto Core no lado do servidor:

```xml
<text name="PhoneBook">Phone Book</text>
<text name="PhoneBooksHeaderInfo">Phone Book Details</text>
```

Para outros idiomas, adicione em arquivos de localização específicos:

```xml
<!-- PhoneBook-tr.xml -->
<text name="PhoneBook">Telefon Rehberi</text>
<text name="PhoneBooksHeaderInfo">Telefon Rehberi Detayları</text>
```

### Rota Angular

Angular tem um poderoso sistema de roteamento de URL. Adicione rotas no módulo apropriado:

```typescript
{
  path: 'phonebook',
  loadComponent: () => import('./phonebook/phonebook.component').then((m) => m.PhoneBookComponent),
}
```

## Criação de Entidade

### Definindo a Entidade

Crie sua classe de entidade no projeto Core:

```csharp
public class Person : FullAuditedEntity
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string EmailAddress { get; set; }
}
```

### Configurando o DbContext

Adicione um DbSet para sua entidade no DbContext:

```csharp
public virtual DbSet<Person> People { get; set; }
```

### Criando Migração de Banco de Dados

Execute o comando para criar uma migração:

```bash
dotnet ef migrations add Added_Person_Entity
```

### Atualizando o Banco de Dados

Execute o comando para aplicar a migração:

```bash
dotnet ef database update
```

## Serviços de Aplicação

### Gerenciamento de Conexão e Transação

Não abrimos manualmente conexões de banco de dados ou iniciamos/commitamos transações manualmente. Isso é feito automaticamente pelo sistema Unit Of Work do framework ABP.

### Tratamento de Exceção

Não tratamos exceções manualmente (usando bloco try-catch). O framework ABP trata automaticamente todas as exceções na camada web e retorna mensagens de erro apropriadas ao cliente.

### Criando um Serviço de Aplicação

```csharp
public class PersonAppService : ApplicationService, IPersonAppService
{
    private readonly IRepository<Person> _personRepository;

    public PersonAppService(IRepository<Person> personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task<PagedResultDto<PersonListDto>> GetPeople(GetPeopleInput input)
    {
        var query = _personRepository.GetAll()
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), 
                p => p.Name.Contains(input.Filter) || p.Surname.Contains(input.Filter));

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(input.Sorting ?? "Name ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<PersonListDto>(
            totalCount,
            ObjectMapper.Map<List<PersonListDto>>(items)
        );
    }
}
```

## Testes Unitários

### Multi-Tenância em Testes

Se você desabilitou multitenância, você deve desabilitá-la também para testes unitários. Defina "MultiTenancyEnabled" como false na classe de constantes do projeto Core.

### Criando Testes

Derive sua classe de teste da classe base apropriada:

```csharp
using Shouldly;
using Xunit;

namespace MyApp.Tests.People
{
    public class PersonAppService_Tests : AppTestBase
    {
        private readonly IPersonAppService _personAppService;

        public PersonAppService_Tests()
        {
            _personAppService = Resolve<IPersonAppService>();
        }

        [Fact]
        public void Should_Get_All_People_Without_Any_Filter()
        {
            // Act
            var persons = _personAppService.GetPeople(new GetPeopleInput());

            // Assert
            persons.Items.Count.ShouldBe(2);
        }

        [Fact]
        public void Should_Get_People_With_Filter()
        {
            // Act
            var persons = _personAppService.GetPeople(
                new GetPeopleInput { Filter = "adams" });

            // Assert
            persons.Items.Count.ShouldBe(1);
            persons.Items[0].Name.ShouldBe("Douglas");
            persons.Items[0].Surname.ShouldBe("Adams");
        }
    }
}
```

A classe base de teste inicializa todo o sistema, cria um banco de dados falso em memória, semeia dados iniciais e faz login como administrador. Isso é na verdade um teste de integração que testa todo o código do lado do servidor.

## Migração de AutoMapper para Mapperly

### Por que Mapperly?

Mapperly oferece várias vantagens sobre AutoMapper:
- Mapeamento em tempo de compilação em vez de tempo de execução
- Melhor performance
- Detecção de erros em tempo de compilação
- Menos sobrecarga de memória

### Mudanças Arquiteturais

**Antes: CustomDtoMapper Centralizado**

```csharp
public static class CustomDtoMapper
{
    public static void CreateMappings(IMapperConfigurationExpression configuration)
    {
        configuration.CreateMap<Person, PersonListDto>();
        configuration.CreateMap<Person, GetPersonForEditOutput>();
        configuration.CreateMap<EditPersonInput, Person>();
        configuration.CreateMap<CreatePersonInput, Person>();
    }
}
```

**Depois: Classes de Mapper por Funcionalidade**

```csharp
[Mapper]
public partial class PersonToPersonListDtoMapper
{
    public partial PersonListDto Map(Person person);
    public partial List<PersonListDto> Map(List<Person> persons);
}

[Mapper]
public partial class PersonToGetPersonForEditOutputMapper
{
    public partial GetPersonForEditOutput Map(Person person);
}

[Mapper]
public partial class EditPersonInputToPersonMapper
{
    public partial void Map(EditPersonInput input, Person person);
}
```

Convenção de nomenclatura: `{SourceType}To{DestinationType}Mapper`

### Passos de Migração

1. **Remover pacotes AutoMapper**
2. **Adicionar pacote Mapperly**
3. **Converter configurações de mapeamento**
4. **Continuar usando ObjectMapper**

### Padrões Comuns de Mapeamento

**Mapeamento Básico:**
```csharp
[Mapper]
public partial class SourceToDestinationMapper
{
    public partial DestinationDto Map(Source source);
}
```

**Mapeamento de Coleção:**
```csharp
[Mapper]
public partial class SourceToDestinationMapper
{
    public partial List<DestinationDto> Map(List<Source> sources);
}
```

**Mapeamento Reverso:**
```csharp
[Mapper]
public partial class SourceToDestinationMapper
{
    [MapProperty(nameof(Source.Name), nameof(DestinationDto.FullName))]
    public partial DestinationDto Map(Source source);
}
```

**Atualizar Objetos Existentes:**
```csharp
[Mapper]
public partial class SourceToDestinationMapper
{
    public partial void Map(SourceDto input, Destination destination);
}
```

**Mapeamento de Propriedade Customizada:**
```csharp
[Mapper]
public partial class SourceToDestinationMapper
{
    [MapProperty(nameof(Source.FirstName), nameof(DestinationDto.FirstName))]
    [MapProperty(nameof(Source.LastName), nameof(DestinationDto.LastName))]
    public partial DestinationDto Map(Source source);
}
```

**Ignorar Propriedades:**
```csharp
[Mapper]
public partial class SourceToDestinationMapper
{
    [MapperIgnoreSource(nameof(Source.Id))]
    public partial DestinationDto Map(Source source);
}
```

## Melhores Práticas

### 1. Nomenclatura

- Use nomes descritivos para classes de módulo
- Siga o padrão `Eaf.{ModuleName}Module`
- Use namespaces consistentes

### 2. Dependências

- Declare todas as dependências explicitamente
- Evite dependências circulares
- Agrupe dependências relacionadas

### 3. Inicialização

- Use `PreInitialize` para configuração
- Use `Initialize` para registro de DI
- Use `PostInitialize` para inicialização que depende de serviços resolvidos

### 4. Configuração

- Use o sistema de configuração do ABP
- Forneça valores padrão sensatos
- Documente todas as opções de configuração

### 5. Testes

- Crie testes para funcionalidades do módulo
- Teste integração com outros módulos
- Teste configuração do módulo

## Referências

- [ASP.NET Boilerplate Module System v10.3](https://aspnetboilerplate.com/Pages/Documents/v10.3/Module-System)
- [ASP.NET Boilerplate Startup Configuration](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration)
- [ASP.NET Boilerplate Dependency Injection](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection)
- [ASP.NET Boilerplate ASP.NET Core](https://aspnetboilerplate.com/Pages/Documents/AspNet-Core)
- [ASP.NET Boilerplate Plugin System](https://aspnetboilerplate.com/Pages/Documents/Plugin)
