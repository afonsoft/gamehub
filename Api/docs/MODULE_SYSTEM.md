# Sistema de Módulos - ASP.NET Boilerplate

## Visão Geral

O ASP.NET Boilerplate fornece uma infraestrutura para construir módulos e compô-los para criar uma aplicação. Um módulo pode depender de outro módulo. Geralmente, um assembly é considerado como um módulo. Se você criou uma aplicação com mais de um assembly, é sugerido criar uma definição de módulo para cada assembly.

O sistema de módulos atualmente é focado no lado do servidor em vez do lado do cliente.

Para documentação oficial completa, consulte: [ASP.NET Boilerplate Module System](https://aspnetboilerplate.com/Pages/Documents/v2.0.0/Module-System)

## Definição de Módulo

Um módulo é definido com uma classe que deriva de `AbpModule`. Digamos que estamos desenvolvendo um módulo Blog que pode ser usado em diferentes aplicações. A definição de módulo mais simples pode ser mostrada abaixo:

```csharp
public class MyBlogApplicationModule : AbpModule
{
    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
    }
}
```

A classe de definição de módulo é responsável por registrar suas classes no sistema de [injeção de dependência](https://aspnetboilerplate.com/Pages/Documents/v2.0.0/Dependency-Injection) (pode ser feito convencionalmente como mostrado acima) se necessário. Ela pode configurar a aplicação e outros módulos, adicionar novos recursos à aplicação e assim por diante.

## Métodos de Ciclo de Vida

O ASP.NET Boilerplate chama alguns métodos específicos dos módulos na inicialização e encerramento da aplicação. Você pode substituir esses métodos para executar tarefas específicas.

O ASP.NET Boilerplate chama esses métodos ordenados por dependências. Se o módulo A depende do módulo B, o módulo B é inicializado antes do módulo A. A ordem exata dos métodos de inicialização: PreInitialize-B, PreInitialize-A, Initialize-B, Initialize-A, PostInitialize-B e PostInitialize-A. Isso é verdadeiro para todo o gráfico de dependências. O método Shutdown também é similar, mas em ordem reversa.

### PreInitialize

Este método é chamado primeiro quando a aplicação inicia. É o método usual para [configurar](https://aspnetboilerplate.com/Pages/Documents/v2.0.0/Startup-Configuration) o framework e outros módulos antes que eles inicializem.

Além disso, você pode escrever código específico aqui para executar antes dos registros de injeção de dependência. Por exemplo, se você cria uma classe de [registro convencional](https://aspnetboilerplate.com/Pages/Documents/v2.0.0/Dependency-Injection), você deve registrá-la aqui usando o método `IocManager.AddConventionalRegisterer`.

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

Enquanto os módulos são investigados começando do módulo de inicialização e indo para as dependências, o ABP também pode carregar módulos dinamicamente. A classe `AbpBootstrapper` define a propriedade `PlugInSources` que pode ser usada para adicionar fontes para carregar dinamicamente módulos plugin. Uma fonte plugin pode ser qualquer classe que implementa a interface `IPlugInSource`. A classe `PlugInFolderSource` implementa isso para obter módulos plugin de assemblies localizados em uma pasta.

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

## Configuração de Módulo

Enquanto métodos de módulo personalizados podem ser usados para configurar módulos, sugerimos usar o sistema de [configuração de inicialização](https://aspnetboilerplate.com/Pages/Documents/v2.0.0/Startup-Configuration) para definir e definir configuração para módulos.

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

- [ASP.NET Boilerplate Module System](https://aspnetboilerplate.com/Pages/Documents/v2.0.0/Module-System)
- [ASP.NET Boilerplate Startup Configuration](https://aspnetboilerplate.com/Pages/Documents/v2.0.0/Startup-Configuration)
- [ASP.NET Boilerplate Dependency Injection](https://aspnetboilerplate.com/Pages/Documents/v2.0.0/Dependency-Injection)
- [ASP.NET Boilerplate ASP.NET Core](https://aspnetboilerplate.com/Pages/Documents/v2.0.0/AspNet-Core)
