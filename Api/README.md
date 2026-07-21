# EAF API Template

Este é um template de projeto API para a plataforma EAF (Enterprise Application Foundation).

## Documentação

Para documentação detalhada sobre o template API, consulte a pasta [docs](docs/).

### Documentação Disponível

- **[Getting Started](docs/GETTING_STARTED.md)** - Instalação, configuração e execução da aplicação
- **[Modules](docs/MODULES.md)** - Estrutura de módulos e organização do projeto
- **[Module System](docs/MODULE_SYSTEM.md)** - Sistema de módulos do ASP.NET Boilerplate e como criar novos módulos
- **[Implementations](docs/IMPLEMENTATIONS.md)** - Padrões de implementação e melhores práticas

## Configuração de Banco de Dados

### Provider de Banco de Dados

O template suporta diferentes providers de banco de dados através da configuração `Database:Provider` em `appsettings.json`:

```json
{
  "Database": {
    "Provider": "SqlServer"
  }
}
```

**Providers suportados:**
- `SqlServer` ou `MSSQL` - Microsoft SQL Server (padrão)
- `MySQL` - MySQL (quando suporte estável para EF Core 10.0 estiver disponível)
- `PostgreSQL` ou `Postgres` - PostgreSQL (quando suporte estável para EF Core 10.0 estiver disponível)

### Hangfire

O Hangfire suporta três tipos de armazenamento, determinados automaticamente com base no provider de banco de dados e configurações de Redis:

A configuração do Hangfire em `appsettings.json` é:

```json
{
  "Hangfire": {
    "IsEnabled": "true",
    "IsInMemoryDatabase": "false"
  }
}
```

O sistema verifica automaticamente:
1. Se `Hangfire:IsEnabled` está configurado como `true`
2. Se `Database:Provider` é `SqlServer` ou `MSSQL`
3. Se `RedisCache:IsEnabled` ou `RedisCache:IsRedisEnabled` está `true`

**Comportamento (lógica de seleção de storage):**
- **SQL Server (`Database:Provider` = `SqlServer` ou `MSSQL`):** Hangfire usa o banco de dados SQL Server para armazenamento de jobs (recomendado para produção)
- **Não SQL Server + Redis habilitado:** Hangfire usa Redis para armazenamento de jobs via `Hangfire.Redis.StackExchange` (recomendado para produção sem SQL Server)
- **Não SQL Server + Redis desabilitado:** Hangfire usa armazenamento em memória (dados são perdidos ao reiniciar a aplicação)
- **`Hangfire:IsInMemoryDatabase` = `true`:** Força armazenamento em memória independente do provider

**Configuração com Redis:**
```json
{
  "RedisCache": {
    "IsEnabled": "true",
    "ConnectionString": "localhost:6379",
    "DatabaseId": 0
  }
}
```

O pacote `Hangfire.Redis.StackExchange` (open source) é utilizado para armazenamento Redis. A connection string e o DatabaseId são compartilhados com as configurações de cache Redis do EAF.

**Nota:** PostgreSQL e MySQL ainda não têm suporte estável para EF Core 10.0. Quando versões estáveis estiverem disponíveis, o suporte será adicionado ao template.

**Hangfire 1.8 Upgrade Notes:**
- O EAF usa Hangfire 1.8.23 com `Microsoft.Data.SqlClient` 7.0.1
- A criptografia é habilitada por padrão em `Microsoft.Data.SqlClient`
- As connection strings devem incluir `TrustServerCertificate=True` para evitar erros de conexão
- Esta configuração já está presente nos arquivos de configuração de produção, staging, local e development

**IIS Configuration for Production (Always Running):**
Para garantir que o Hangfire execute continuamente em produção, configure o IIS da seguinte forma:

1. **Application Pool Settings:**
   - .NET CLR version: `.NET CLR Version v4.0.30319`
   - Managed pipeline mode: `Integrated`
   - Start mode: `Always Running`
   - Idle Time-Out (minutes): `0`

2. **Application Settings:**
   - Preload Enabled: `True`

3. **Application Initialization Configuration:**
   - doAppInitAfterRestart: `True`
   - Adicionar initialization page: `/hangfire`

Isso garante que o Hangfire Server inicie automaticamente após o reciclagem do IIS e continue processando jobs mesmo durante períodos de inatividade.

## Docker

A imagem Docker do template API está em `Templates/Api/Dockerfile` e deve ser buildada a partir da raiz do repositório, pois o template referencia os projetos-fonte do EAF em `src/`.

### Build

```bash
cd /caminho/para/EAF
docker build -t eaf-api -f Templates/Api/Dockerfile .
```

### Execução

As configurações sensíveis (connection string) e opcionais são passadas por variáveis de ambiente no momento da execução:

```bash
docker run -d -p 8001:8001 \
  -e ConnectionStrings__Default="Server=...,1433;Database=...;user id=...;Password=...;TrustServerCertificate=True;Encrypt=false" \
  -e Database__Provider=SqlServer \
  -e Hangfire__IsEnabled=false \
  -e SqlServerCache__IsEnabled=false \
  -e RedisCache__IsEnabled=false \
  -e App__ServerRootAddress=http://localhost:8001/ \
  -e App__ClientRootAddress=http://localhost:8000/ \
  -e App__CorsOrigins=* \
  eaf-api
```

### docker-compose

```bash
cd Templates/Api
ConnectionStrings__Default="..." docker compose up --build
```

As variáveis são lidas pelo `ASPNETCORE_ENVIRONMENT=Production` e sobrescrevem as configurações do `appsettings.json`. Os arquivos `appsettings.{Local|Staging|Production|Development}.json` são removidos da imagem para evitar conexões padrão.

#### Variáveis de ambiente

Copie `.env.example` para `.env` e ajuste os valores:

```bash
cp .env.example .env
```

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `ConnectionStrings__Default` | Connection string do SQL Server | `Server=sqlserver,1433;Database=EafProjectName;User Id=sa;Password=...` |
| `Database__Provider` | Provider do banco de dados | `SqlServer` |
| `Hangfire__IsEnabled` | Habilita o Hangfire | `false` |
| `SqlServerCache__IsEnabled` | Habilita cache no SQL Server | `false` |
| `RedisCache__IsEnabled` | Habilita cache no Redis | `true` |
| `RedisCache__ConnectionString` | Connection string do Redis | `redis:6379` |
| `RedisCache__DatabaseId` | Database ID do Redis | `0` |
| `App__ServerRootAddress` | URL raiz da API | `http://localhost:8001/` |
| `App__ClientRootAddress` | URL raiz do client | `http://localhost:8000/` |
| `App__CorsOrigins` | Origens permitidas pelo CORS | `*` |

#### Simulando Redis e SQL Server localmente

Para testar a API com Redis e SQL Server sem depender de infraestrutura externa, use o arquivo opcional `docker-compose.infra.yml`:

```bash
cd Templates/Api
ConnectionStrings__Default="Server=sqlserver,1433;Database=EafProjectName;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;Encrypt=false" \
RedisCache__IsEnabled=true \
docker compose -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.infra.yml up -d
```

Esse comando sobe:
- `eaf-sqlserver`: SQL Server 2022 Express
- `eaf-redis`: Redis 7
- `GameHub.web.host`: API EAF conectada aos dois

Aplique as migrations no SQL Server antes de acessar a API pela primeira vez:

```bash
docker compose -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.infra.yml exec GameHub.web.host dotnet ef database update
```

## Estrutura do Projeto

```
├── src/
│   ├── GameHub.Core/          # Entidades de domínio e lógica de negócio
│   ├── GameHub.Application/   # Serviços de aplicação e DTOs
│   ├── GameHub.EntityFrameworkCore/  # Acesso a dados e migrações
│   └── GameHub.Web.Host/      # Host da API Web
└── test/
    ├── GameHub.Tests/          # Testes de integração
    └── GameHub.Web.Tests/      # Testes Web
```

## Build e Testes

### Build
```bash
dotnet build GameHub.sln
```

### Testes
```bash
dotnet test GameHub.sln
```

### Testes com Cobertura
```bash
dotnet test GameHub.sln --collect:"XPlat Code Coverage"
```

## Status dos Testes

| Camada | Arquivo de Testes | Qtd | Status |
|--------|-------------------|-----|--------|
| Domain (Core) | `AirplaneManager_Crud_Tests` | 8 | Passando |
| Domain (Core) | `Airplane_Validation_Tests` | 7 | Passando |
| Domain (Core) | `ProjectNameConsts_Validation_Tests` | 4 | Passando |
| Application | `AirplanesAppService_Crud_Tests` | 10 | 9 passando, 1 skip (EPPlus) |
| Application | `ProjectNameAuthorizationProvider_Permissions_Tests` | 5 | Passando |
| DTOs | `CreateOrEditAirplaneDto_Validation_Tests` | 6 | Passando |
| DTOs | `GetAirplanesInput_Validation_Tests` | 4 | Passando |
| DTOs | `AirplaneDto_Mapping_Tests` | 2 | Passando |
| Existentes | Testes pré-existentes (MultiTenancy, Roles, Users, etc.) | 131 | Passando (1 skip) |
| **Total** | | **177** | **175 passando, 2 skipped** |

## Módulos Implementados

### Airplanes (Exemplo)
- **Entidade**: `Airplane` - Representa uma aeronave
- **Serviço**: `AirplanesAppService` - CRUD de aeronaves
- **DTOs**: 
  - `AirplaneDto` - DTO de leitura
  - `CreateOrEditAirplaneDto` - DTO de criação/edição
  - `GetAirplanesInput` - DTO de entrada para consulta
- **Exportação**: `AirplanesExcelExporter` - Exportação para Excel

## Tecnologias

- **.NET 10.0**
- **ASP.NET Core 10.0**
- **Entity Framework Core 10.0**
- **ABP Framework**
- **xUnit** (Testes)
- **Shouldly** (Assertions)

### Padrão de Testes (BDD)

Os testes seguem o padrão BDD em português:
- **Dado** (Given) - Preparação do cenário
- **Quando** (When) - Ação sendo testada
- **Então** (Then) - Verificação do resultado

Nomenclatura: `Dado_{Contexto}_Quando_{Acao}_Entao_{Resultado}`

### Frameworks de Teste
- **xUnit** - Framework de testes
- **Shouldly** - Assertions fluentes
- **NSubstitute** - Mocking
- **Abp.TestBase** - Base de testes ABP com UoW e DI

## Próximos Passos

- [ ] Aumentar cobertura de código para 90%
- [ ] Adicionar documentação XML aos membros públicos
- [ ] Migrar EPPlus para API `ExcelPackage.License` (v8+)
- [ ] Adicionar testes de integração Web (GameHub.Web.Tests)

---

Desenvolvido com [EAF](https://github.com/afonsoft/EAF)
