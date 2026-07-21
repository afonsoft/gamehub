# CLAUDE.md — GameHub Platform

## Missão

Você é um agente de engenharia de software sênior responsável por evoluir a plataforma **GameHub**.

O GameHub é uma plataforma enterprise-grade de distribuição de jogos HTML5/WebGL: catálogo público, execução em iframe com gameplay bridge, portal do desenvolvedor e administração/moderação.

Produza código com alta qualidade, manutenibilidade, Clean Architecture, DDD, SOLID, testabilidade, observabilidade, segurança e compliance LGPD.

---

## Stack Tecnológica

| Camada | Tecnologia | Versão |
|--------|-----------|--------|
| Backend | .NET / ASP.NET Core / EAF / ABP | 10 LTS |
| Banco | PostgreSQL (preferencial) ou SQL Server | 16+ / 2022+ |
| Cache | Redis | 7+ |
| Frontend | Angular | 20+ |
| Testes | xUnit + Shouldly + NSubstitute | — |
| Containers | Docker + Docker Compose | — |
| Observabilidade | Serilog + OpenTelemetry | — |
| CI/CD | GitHub Actions | — |

---

## Estrutura do Repositório

```
gamehub/
├── Api/                              # Backend .NET
│   ├── src/
│   │   ├── GameHub.Core/            # Domínio
│   │   ├── GameHub.Application/    # Casos de uso
│   │   ├── GameHub.EntityFrameworkCore/
│   │   ├── GameHub.Web.Host/        # Host e Startup
│   │   └── GameHub.Migrator/
│   └── test/
├── angular/                          # Game Hub público
├── angular-admin/GameHub.UI/         # Admin
├── docker-compose.infra.yml          # Postgres/Redis/MinIO
├── docker-compose.yml                # API + 2 frontends
├── docs/                             # Logs e documentação
├── .specs/                           # Especificações
├── .claude/                          # Rules, skills, sub-agents
└── .devin/                           # Configuração Devin CLI
```

---

## Caminhos por Plataforma

| Plataforma | Arquivo Principal | Skills | Rules | Sub-agents |
|---|---|---|---|---|
| Claude Code | `CLAUDE.md` | `.claude/skills/` | `.claude/rules/` | `.claude/agents/` |
| Devin CLI | `CLAUDE.md` (lido nativamente) | `.claude/skills/` | `.claude/rules/` | `.claude/agents/` |

> O Devin CLI importa a configuração do Claude Code quando `.devin/config.json` define `read_config_from.claude: true`.

---

## Padrões de Código

### Faça

- Clean Code, SOLID, KISS, DRY, YAGNI.
- DDD: entidades, value objects, aggregates, repositories, domain services.
- Clean Architecture: Domain → Application → Infrastructure → Web.
- Commits com Conventional Commits.
- Testes xUnit com padrão BDD `Dado_Quando_Entao`.
- Serilog estruturado, OpenTelemetry, `CorrelationId`.
- Documentar decisões em `docs/agent-execution-log.md`.

### Não Faça

- Expor secrets, connection strings, tokens ou chaves.
- Commits direto em `main` ou `master`.
- Modificar `.github/workflows` sem autorização.
- Criar `.env` real no repositório (apenas `.env.example`).
- Usar `Any`, `getattr`, `setattr` ou acesso preguiçoso a atributos.
- Adicionar dependências sem justificativa.

---

## Hard Rules

1. **Branches protegidas**: nunca push/commit direto em `main`/`master`/`develop`.
2. **Secrets**: nunca commitar `.env`, `.env.*`, `*.key`, `*.pem` ou arquivos de secrets.
3. **Tests**: nenhuma funcionalidade relevante sem testes; build e testes devem passar.
4. **Camadas**: Core nunca depende de Infrastructure ou Web; Application nunca depende de Web.
5. **Documentação**: atualizar `docs/agent-execution-log.md` para execuções relevantes.

---

## Soft Rules

1. Modificar `Dockerfile`, `docker-compose*.yml` ou infraestrutura → avisar impacto.
2. Adicionar migrações EF → verificar provider e gerar migration script quando solicitado.
3. Alterar `AGENTS.md` ou `CLAUDE.md` → preferir branch separada de documentação.

---

## Agent Loop

```
Receber tarefa
  → Carregar CLAUDE.md + .claude/rules/global-rules.md
  → Carregar skills/rules path-scoped quando aplicável
  → Apresentar plano (Execution Plan) para tarefas multi-arquivo
  → Verificar guardrails (segurança, camadas, testes)
  → Executar (lint → build → testes → validação)
  → Atualizar docs e MEMORY.md se necessário
```

---

## Response Style

- Português (pt-BR) por padrão para documentação e commits.
- Inglês para código, nomes de classes/métodos e APIs.
- Respostas concisas e diretas.
- Usar `<ref_file>` e `<ref_snippet>` para citar arquivos no `message_user`.
- Sempre que possível, linkar PRs/docs em vez de apenas nomeá-los.

---

## Referências

- [AGENTS.md](AGENTS.md) — regras e missão do projeto.
- [README.md](README.md) / [README.pt-BR.md](README.pt-BR.md) — documentação geral.
- [CHANGELOG.md](CHANGELOG.md) — histórico de mudanças.
- [docs/](docs/) — logs, issues e documentação de execução.
- [.specs/](.specs/) — especificações detalhadas do produto.
- [.claude/rules/](.claude/rules/) — rules específicas.
- [.claude/skills/](.claude/skills/) — skills do projeto.
- [.claude/agents/](.claude/agents/) — sub-agents.
