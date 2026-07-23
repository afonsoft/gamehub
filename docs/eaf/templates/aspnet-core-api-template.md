# ASP.NET Core API Template

## Visão Geral

O template ASP.NET Core API fornece uma API REST completa usando .NET 10.0, ASP.NET Core, e integração com o framework EAF.

## Tecnologias

- **.NET 10.0**: Framework .NET mais recente
- **C# 14**: Linguagem C# mais recente
- **ASP.NET Core**: Framework web
- **Entity Framework Core**: ORM para acesso a dados
- **ASP.NET Boilerplate**: Framework base

## Estrutura do Projeto

```
Templates/Api/
├── src/
│   ├── Eaf.ProjectName.Application/    # Camada de Aplicação
│   │   ├── Services/                   # Application Services
│   │   └── Dtos/                       # Data Transfer Objects
│   ├── Eaf.ProjectName.Core/          # Camada de Domínio
│   │   ├── Entities/                  # Entidades
│   │   └── Interfaces/                # Interfaces de Repositório
│   └── Eaf.ProjectName.Web.Host/      # Camada de Apresentação (API)
│       └── Controllers/                # Controllers
└── test/                               # Projetos de Teste
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
    "Default": "Server=localhost;Database=EafProjectName;Trusted_Connection=True"
  }
}
```

### 3. Executar Migrations

```bash
dotnet ef database update
```

### 4. Executar o Projeto

```bash
dotnet run --project src/Eaf.ProjectName.Web.Host/Eaf.ProjectName.Web.Host.csproj
```

A API estará disponível em `http://localhost:21021`

## Padrões EAF API

### Camadas do EAF

O template segue a arquitetura N-Layer do ASP.NET Boilerplate:

- **Core**: Entidades, value objects, interfaces de repositório
- **Application**: Application services, DTOs, validação
- **EntityFrameworkCore**: Implementação de repositórios e DbContext
- **Web.Host**: API controllers, configuração ASP.NET Core

### Application Services

Os application services são o ponto principal de entrada da API:

```csharp
public class MyEntityAppService : ApplicationService, IMyEntityAppService
{
    private readonly IRepository<MyEntity, Guid> _myEntityRepository;

    public MyEntityAppService(IRepository<MyEntity, Guid> myEntityRepository)
    {
        _myEntityRepository = myEntityRepository;
    }

    public async Task<MyEntityDto> CreateAsync(CreateMyEntityDto input)
    {
        var entity = ObjectMapper.Map<MyEntity>(input);
        await _myEntityRepository.InsertAsync(entity);
        return ObjectMapper.Map<MyEntityDto>(entity);
    }
}
```

### DTOs

Os DTOs são usados para entrada e saída de dados:

```csharp
public class MyEntityDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class CreateMyEntityDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }
}
```

### Autorização

Use o atributo `[AbpAuthorize]` para proteger endpoints:

```csharp
[AbpAuthorize("Pages.MyEntity")]
public class MyEntityAppService : ApplicationService, IMyEntityAppService
{
    // ...
}
```

## Swagger/OpenAPI

O template inclui Swagger para documentação da API:

- Acesse `http://localhost:21021/swagger` para ver a documentação
- Use o endpoint `/swagger/v1/swagger.json` para obter o schema

## Módulos EAF

O template inclui os módulos EAF principais:

- **Eaf.Middleware.Core**: Módulo base
- **Eaf.Middleware.Application**: Camada de aplicação
- **Eaf.Middleware.Web.Core**: Configurações web
- **Eaf.Castle.Serilog**: Logging estruturado
- **Eaf.KeyVault**: Gerenciamento de segredos (opcional)
- **Eaf.OpenTelemetry**: Observabilidade (opcional)

## Testes

O template inclui projetos de teste:

- **Eaf.ProjectName.Application.Tests**: Testes da camada de aplicação
- **Eaf.ProjectName.EntityFrameworkCore.Tests**: Testes de acesso a dados

Execute os testes:

```bash
dotnet test
```

## Docker

O template inclui suporte a Docker:

```bash
docker build -t eaf-api .
docker run -p 21021:21021 eaf-api
```

## Troubleshooting

- **Erro de Conexão**: Verifique a connection string no appsettings.json
- **Migrations**: Execute `dotnet ef migrations add` para criar novas migrations
- **Permissões**: Verifique se o usuário tem permissões no banco de dados
