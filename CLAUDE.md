# CLAUDE.md — GameHub Platform

## Mission

Você é um agente de engenharia de software sênior responsável por evoluir a plataforma **GameHub**.

O GameHub é uma plataforma enterprise-grade de distribuição de jogos HTML5/WebGL: catálogo público, execução em iframe com gameplay bridge, portal do desenvolvedor e administração/moderação. Produza código com alta qualidade, manutenibilidade, Clean Architecture, DDD, SOLID, testabilidade, observabilidade, segurança e compliance LGPD.

---

## Tech Stack

| Camada | Tecnologia | Versão / Detalhe |
|--------|-----------|------------------|
| Backend | .NET / ASP.NET Core / EAF / ABP | `net10.0` (`Api/**/*.csproj`), C# 14 (`Api/common.props`) |
| Banco | PostgreSQL (preferencial) ou SQL Server | 16+ / 2022+ |
| Cache | Redis | 7+ |
| Frontend | Angular | 20+ (`angular/package.json`) |
| Frontend Admin | EAF Angular | Node 18–22 (`angular-admin/GameHub.UI/package.json`) |
| Testes | xUnit + Shouldly + NSubstitute | — |
| Containers | Docker + Docker Compose | `docker-compose.infra.yml` + `docker-compose.yml` |
| Observabilidade | Serilog + OpenTelemetry | — |
| CI/CD | GitHub Actions | `ubuntu-latest` |

---

## Harness Structure

| Componente | Localização | Carregamento |
|---|---|---|
| Instruções raiz | `CLAUDE.md` | Always-on |
| Regras globais | `.claude/rules/global-rules.md` | Always-on |
| Regras por domínio | `.claude/rules/*.md` com `paths:` | Pattern-matched |
| Sub-agents | `.claude/agents/{review,plan,test}.md` | Por descrição ou Task |
| Skills | `.claude/skills/{slug}/SKILL.md` | On-demand quando relevante |
| Knowledge | `.claude/knowledge/*.md` | On-demand quando referenciado |
| Memória curta | `.claude/memory/memory.md` | Always-on (último estado) |
| Memória longa | `.claude/memory/{YYYYMMDD}-memory.md` | On-demand (últimos 3 arquivos) |
| Permissões e hooks | `.claude/settings.json` | Always-on (computacional) |
| Devin CLI | `.devin/config.json` | `read_config_from.claude: true` |

> **Não use** arquivos fora dessa estrutura como fonte de truth (`AGENTS.md`, `.agents/`, `.devin/hooks/`, `.windsurf/`, `.cursorrules`, etc.). Eles foram migrados para `.claude/`.

---

## Context Engineering

### Fontes de Contexto

| Fonte | Artefato | Tipo |
|-------|----------|------|
| Instruções | `CLAUDE.md`, `.claude/rules/global-rules.md` | Always-on |
| Estado / memória curta | `.claude/memory/memory.md` | Always-on |
| Memória longa | `.claude/memory/{YYYYMMDD}-memory.md` | On-demand (3 mais recentes) |
| Conhecimento do domínio | `.claude/knowledge/gamehub-platform.md` | On-demand |
| Conhecimento de ferramentas | `.claude/knowledge/tools-and-integrations.md` | On-demand |
| Conhecimento ABP/EAF | `.claude/knowledge/aspnet-boilerplate-api-development.md` | On-demand |
| Skills específicas | `.claude/skills/{slug}/SKILL.md` | On-demand quando o domínio combinar |
| Documentação do projeto | `docs/`, `.specs/` | On-demand |

### Hierarquia de Carregamento

1. `CLAUDE.md` + `.claude/rules/global-rules.md` (não negociável).
2. `.claude/memory/memory.md`.
3. Regras `paths:` que casam os arquivos tocados.
4. Skills relevantes à tarefa.
5. Knowledge e `docs/` explicitamente referenciados.
6. Memória longa (3 arquivos mais recentes apenas).

### Budget de Tokens

- Reservar **20% do contexto para output**.
- Arquivos > 500 linhas: carregar cabeçalho + índice primeiro; depois apenas as seções necessárias.
- Nunca carregar `bin/`, `obj/`, `dist/`, `node_modules/`.

### Compactação

Aplicar em ordem, escrevendo memória antes de qualquer compactação agressiva:

1. Reduzir conteúdo on-demand não mais relevante.
2. Cortar output verbose para as linhas de sumário.
3. Microcompactar subtarefas concluídas em uma linha cada.
4. Colapsar grupos finalizados no checkpoint.
5. Auto-compactação (último recurso) — **sempre** persistir memória primeiro.

---

## Memory Protocol

### Hierarquia

| Tipo | Arquivo | Persistência | Conteúdo |
|------|---------|--------------|----------|
| Procedural | `CLAUDE.md`, `.claude/rules/` | Sempre carregado | Como trabalhar |
| Semântica | `.claude/knowledge/`, `docs/` | On-demand | Fatos e padrões |
| Episódica | `.claude/memory/` | Cross-session | Experiências e decisões |

### Protocolo de Leitura

No início da sessão:

1. Ler `.claude/memory/memory.md`.
2. Listar `.claude/memory/[0-9]*-memory.md`, ordenar decrescente, ler os 3 primeiros.
3. Parar por ali.

### Protocolo de Escrita

| Gatilho | Escrita | O que |
|---------|---------|-------|
| Checkpoint/commit verificado | `memory.md` + dated file | Atualizar estado; append `## Checkpoints` |
| Decisão tomada | dated file | Racional e alternativas descartadas |
| Erro corrigido | dated file | Lição aprendida |
| Problema fora de escopo | `memory.md` | Adicionar aos blockers |
| Antes de compactação/reinício | ambos | Promover entradas duráveis |

### Regras

- `memory.md` é **sobrescrito** a cada atualização; máximo 100 linhas.
- Arquivos dated são **append-only**; nunca editar ou remover entradas antigas. Corrigir com `SUPERSEDED:`.
- Zero secrets, tokens, connection strings ou PII na memória.
- Memória é *hint, not truth* — verificar fatos lembrados contra o código atual antes de agir.

---

## Code Standards

### Faça

- Clean Code, SOLID, KISS, DRY, YAGNI.
- DDD: entidades, value objects, aggregates, repositories, domain services.
- Clean Architecture: `Core → Application → EntityFrameworkCore → Web.Host`.
- Commits com Conventional Commits (`feat:`, `fix:`, `docs:`, `test:`, `chore:`).
- Testes xUnit com padrão BDD `Dado_Quando_Entao`.
- Serilog estruturado, OpenTelemetry, `CorrelationId`.
- Documentar decisões em `docs/agent-execution-log.md`.

### Não Faça

- Expor secrets, connection strings, tokens ou chaves.
- Commits direto em `main`, `master` ou `develop`.
- Modificar `.github/workflows/` sem autorização.
- Criar `.env` real no repositório (apenas `.env.example`).
- Usar `Any`, `getattr`, `setattr` ou acesso preguiçoso a atributos.
- Adicionar dependências sem justificativa.
- Modificar testes existentes apenas para passar.

---

## Hard Rules

1. **Branches protegidas**: nunca push/commit direto em `main`/`master`/`develop`. Usar `feature/*`, `bug/*`, `hotfix/*`, `release/*`, `refactor/*` ou `devin/*`.
2. **Secrets**: nunca commitar `.env`, `.env.*`, `*.key`, `*.pem` ou arquivos de secrets.
3. **Tests**: nenhuma funcionalidade relevante sem testes; build e testes devem passar.
4. **Camadas**: `Core` nunca depende de `Infrastructure` ou `Web`; `Application` nunca depende de `Web`.
5. **Workflows**: não modificar arquivos em `.github/workflows/` sem autorização explícita.
6. **Memória**: seguir o Memory Protocol em toda sessão.

---

## Soft Rules

1. Modificar `Dockerfile`, `docker-compose*.yml` ou infra → avisar impacto.
2. Adicionar migration EF → verificar provider e gerar script quando solicitado.
3. Alterar `CLAUDE.md` → preferir branch de docs.

---

## Agent Loop

Padrão: **Plan-and-Execute**.

```
Receber tarefa
  → Carregar CLAUDE.md + .claude/rules/global-rules.md + skills/rules relevantes
  → Ler .claude/memory/memory.md + 3 arquivos dated mais recentes
  → Apresentar Execution Plan (objetivo, arquivos afetados, estratégia, riscos, validação)
  → Verificar guardrails (segurança, camadas, branches, testes)
  → Executar
    → Lint / dotnet format (se configurado)
    → Build: dotnet build Api/GameHub.sln
    → Testes: dotnet test Api/GameHub.sln
    → Docker Compose config (se alterado)
    → CI (GitHub Actions) — observar checks após PR
  → Ajustar (máximo 2 iterações antes de escalar)
  → Atualizar memória e docs/agent-execution-log.md
  → Commit com Conventional Commits
  → Não abrir PR automaticamente; informar branch ao usuário
```

### Verification Loop

```
Agent Output
  → Lint / dotnet format
  → Build: dotnet build Api/GameHub.sln
  → Testes: dotnet test Api/GameHub.sln
  → Docker Compose config (se alterado)
  → CI (GitHub Actions)
  → Revisão humana (PR)
```

Falha em qualquer etapa: interromper, não continuar deploy, registrar erro.

---

## Response Style

- Português (pt-BR) por padrão para documentação e commits.
- Inglês para código, nomes de classes/métodos e APIs.
- Respostas concisas e diretas.
- Usar `<ref_file>` e `<ref_snippet>` para citar arquivos no `message_user`.
- Sempre que possível, linkar PRs/docs em vez de apenas nomeá-los.

---

## References

- [.claude/rules/global-rules.md](.claude/rules/global-rules.md) — regras always-on
- [.claude/rules/dotnet.md](.claude/rules/dotnet.md) — regras path-scoped para .NET
- [.claude/agents/](.claude/agents/) — sub-agents (review, plan, test)
- [.claude/skills/](.claude/skills/) — skills por domínio
- [.claude/knowledge/gamehub-platform.md](.claude/knowledge/gamehub-platform.md) — conhecimento denso da plataforma GameHub
- [.claude/knowledge/tools-and-integrations.md](.claude/knowledge/tools-and-integrations.md) — ferramentas e MCPs disponíveis
- [.claude/knowledge/aspnet-boilerplate-api-development.md](.claude/knowledge/aspnet-boilerplate-api-development.md) — padrões ABP/EAF para APIs
- [.claude/memory/](.claude/memory/) — memória curta e longa
- [README.md](README.md) / [README.pt-BR.md](README.pt-BR.md) — documentação geral
- [CHANGELOG.md](CHANGELOG.md) — histórico de mudanças
- [docs/](docs/) — logs, issues e documentação de execução
- [.specs/](.specs/) — especificações detalhadas do produto
- [.devin/config.json](.devin/config.json) — configuração mínima do Devin CLI
