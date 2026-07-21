# MEMORY.md — Estado Cross-Session

## Decisões Técnicas

| Data | Decisão | Motivo | Alternativas Descartadas |
|------|---------|--------|--------------------------|
| 2026-07-21 | PostgreSQL como provider principal | Docker Compose usa Postgres | SQL Server (não roda no container padrão) |
| 2026-07-21 | Docker Compose dividido em infra + app | Permitir reiniciar app sem perder dados | Compose único |
| 2026-07-21 | `coverlet.collector` via `--collect:"XPlat Code Coverage"` | Métrica de cobertura padrão do .NET | SonarQube/SonarCloud |

## Débitos Técnicos

| Item | Impacto | Prioridade |
|------|---------|------------|
| Cobertura de testes baixa (10,22%) | Risco de regressões | Alta |
| InMemoryGameCatalogCache temporário | Performance em produção | Média |
| GameHub.Web.Tests com testes skipped | Cobertura web não confiável | Média |

## Lições Aprendidas

- O design-time factory `ProjectNameDbContextFactory` deve setar `SkipMigrate = true` para `dotnet ef migrations add` funcionar.
- O Dockerfile precisa apontar para `GameHub.Web.Host.csproj`/`GameHub.Web.Host.dll`, não para o template antigo.

## Política de Limpeza

- Apagar memórias de branches deletadas após merge.
- Revalidar fatos técnicos contra o código antes de reutilizar.
