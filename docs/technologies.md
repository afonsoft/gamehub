# Tecnologias e Versões — GameHub

## Backend

| Tecnologia | Versão | Uso |
|------------|--------|-----|
| .NET / ASP.NET Core | 10 LTS | API e host |
| EAF / ABP | 10.4 (NuGet EAF.* 9.3.0) | Modularização, multi-tenancy, autorização |
| Entity Framework Core | 10.0.8 | Acesso a dados |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.0 | Provider PostgreSQL |
| AutoMapper | 14.0.0 | Mapeamento DTOs |
| Hangfire | 1.8.x | Background jobs |
| Serilog + sinks | 4.x | Logs estruturados |
| OpenTelemetry | 1.x | Traces e métricas |

## Frontend

| Tecnologia | Versão | Uso |
|------------|--------|-----|
| Angular | 20+ | SPA Hub e Admin |
| TypeScript | strict | Linguagem |
| RxJS | 7.x | Reatividade |
| PrimeNG / Bootstrap | 17+ / 5.x | Componentes Admin |

## Infraestrutura

| Serviço | Versão | Uso |
|---------|--------|-----|
| PostgreSQL | 16+ | Banco de dados |
| Redis | 7+ | Cache, leaderboards, rate limiting |
| MinIO | latest | Object storage local |
| Docker / Docker Compose | — | Containers |

## Testes

| Tecnologia | Versão | Uso |
|------------|--------|-----|
| xUnit | 2.9.x | Testes .NET |
| Shouldly | 4.3.x | Asserts |
| NSubstitute | 5.3.x | Mocks |
| coverlet.collector | 10.0.0 | Cobertura |
