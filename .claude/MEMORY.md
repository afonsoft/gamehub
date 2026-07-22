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

- O design-time factory `GameHubDbContextFactory` deve setar `SkipMigrate = true` para `dotnet ef migrations add` funcionar.
- O Dockerfile precisa apontar para `GameHub.Web.Host.csproj`/`GameHub.Web.Host.dll`, não para o template antigo.

## Ferramentas e Recursos Disponíveis

### Devin Tools Nativas
- `exec`, `read`, `write`, `edit`, `MultiEdit`, `grep`, `find_file_by_name`, `skill`, `todo_write`, `message_user`.
- `git_*` (operações de PR, merge, comentários, labels), `deploy` (frontend/backend).
- `mcp_tool` (acesso a MCP servers), `devin_mcp` / `devin_docs` (gestão do ambiente Devin).
- `web_search` / `web_get_contents` (pesquisa web), `upload_attachment` / `download_attachment`.

### MCP Servers Disponíveis
- `deepwiki`: documentação de repositórios GitHub.
- `firecrawl`: extração de conteúdo web em escala.
- `microsoft-learn`: documentação Microsoft oficial.
- `monday`: gestão de boards e itens.
- `notion`: gestão de páginas e databases.
- `sonarqube`: análise de qualidade de código.
- `tavily`: busca web em tempo real.

### Skills Relevantes
- ABP/EAF: `abp-core`, `abp-ef-core`, `abp-angular`, `abp-testing`, `abp-multi-tenancy`, `abp-application-layer`, `abp-ddd`, `abp-development-flow`.
- .NET/infra: `dotnet-github-actions`, `aspnet-core-api`, `entity-framework-core`, `postgresql-optimization`, `security-jwt`, `modern-csharp-coding-standards`.
- Agent workflow: `create-agent-harness`, `writing-skills`, `harness-repo-structure`, `verification-before-completion`, `systematic-debugging`, `receiving-code-review`.
- Comunicação: `caveman`, `caveman-commit`, `caveman-review`.

### Secrets Configurados
- `FIRECRAWL_API_KEY`, `GITHUB_PAT`, `OMNIROUTE_API_KEY`, `SONARQUBE_TOKEN`.

### Repositório Central de Skills
- `afonsoft/agents-skills` — source of truth para skills, rules e conhecimento reutilizável em todos os projetos afonsoft.

## Política de Limpeza

- Apagar memórias de branches deletadas após merge.
- Revalidar fatos técnicos contra o código antes de reutilizar.
