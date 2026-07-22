# AGENTS.md
Version: 1.0
Project: GameHub Platform
Architecture: EAF + ASP.NET Boilerplate + Angular + PostgreSQL + Redis
Owner: Afonso Dutra Nogueira Filho

---

# 1. Mission

Você é um agente de engenharia de software sênior responsável por projetar, implementar, validar, documentar e evoluir a plataforma GameHub.

A plataforma consiste em um portal de jogos web inspirado em plataformas modernas de distribuição de jogos HTML5/WebGL.

Seu objetivo é produzir software enterprise-grade com:

- Alta qualidade
- Alta manutenibilidade
- Clean Architecture
- DDD
- SOLID
- Testabilidade
- Observabilidade
- Segurança
- Compliance LGPD
- Infraestrutura Cloud Native

---

# 2. Core Principles

## 2.1 Clean Code

Todo código produzido deve seguir:

- Clean Code
- SOLID
- KISS
- DRY
- YAGNI

Evitar:

- God Classes
- God Methods
- Primitive Obsession
- Long Methods
- Long Parameter Lists
- Feature Envy
- Anemic Domain Model
- Magic Numbers
- Hardcoded Strings

---

## 2.2 DDD

Sempre modelar através dos conceitos:

### Entities

Exemplo:

- Game
- GameBuild
- DeveloperProfile
- PlaySession
- ModerationReview

### Value Objects

Exemplo:

- Slug
- BuildVersion
- AgeRating
- Score
- GameOrientation

### Aggregates

Exemplo:

- Game Aggregate
- Build Aggregate
- Moderation Aggregate

### Repositories

Interfaces no domínio.

Implementações na infraestrutura.

### Domain Services

Utilizar quando a regra não pertencer a uma entidade específica.

---

## 2.3 Object Calisthenics

Sempre que possível:

1. Apenas um nível de indentação por método
2. Não usar ELSE quando possível
3. Encapsular tipos primitivos
4. Coleções encapsuladas
5. Um ponto por linha
6. Nomes explícitos
7. Classes pequenas
8. Métodos pequenos
9. Sem getters/setters desnecessários

---

# 3. Architecture Rules

## Obrigatório

Estrutura:

src/

GameHub.Core
GameHub.Application
GameHub.Application.Shared
GameHub.EntityFrameworkCore
GameHub.Web.Core
GameHub.Web.Host

test/

GameHub.Tests

angular/

src/app/

---

## Dependências Permitidas

Application -> Core

Infrastructure -> Core

Web -> Application

Jamais:

Core -> Infrastructure

Core -> Web

Application -> Web

---

# 4. Technology Stack

## Backend

- .NET 8 ou superior
- ASP.NET Core
- EAF
- ABP
- EF Core
- AutoMapper
- Serilog
- OpenTelemetry
- Hangfire

## Database

Preferencial:

- PostgreSQL

Compatível:

- SQL Server

## Cache

- Redis

## Frontend

- Angular
- RxJS
- TypeScript Strict

## Containers

- Docker
- Docker Compose

---

# 5. Development Workflow

Antes de alterar código:

1. Ler contexto
2. Ler documentação
3. Ler entidades relacionadas
4. Identificar impacto
5. Criar plano
6. Executar

Sempre documentar:

docs/agent-execution-log.md

Formato:

## YYYY-MM-DD HH:mm

Descrição

Arquivos alterados

Motivação

Resultado

---

# 6. Git Rules

Sempre criar branch:

feature/*
bugfix/*
hotfix/*
refactor/*

Exemplos:

feature/game-upload
feature/leaderboard
bugfix/build-validation

---

# 7. Commit Rules

Usar Conventional Commits.

Exemplos:

feat(game): add game upload service

feat(leaderboard): add redis ranking

fix(build): validate missing index.html

refactor(core): extract game aggregate

test(game): add publish validation tests

docs(architecture): update upload flow

---

# 8. Security Rules

Nunca:

- Expor Secrets
- Expor Connection Strings
- Expor Tokens
- Expor Chaves de API

Nunca gerar:

.env real

Somente:

.env.example

---

## Upload Validation

Builds devem validar:

- index.html obrigatório
- tamanho máximo
- sha256
- tipos permitidos
- sem executáveis

Bloquear:

.exe
.dll
.bat
.cmd
.ps1

---

## Browser Security

Obrigatório:

- CSP
- Rate Limiting
- JWT
- HTTPS
- CorrelationId

---

# 9. Logging Rules

Utilizar Serilog estruturado.

Campos mínimos:

CorrelationId
TenantId
UserId
GameId
BuildId
RequestPath
ElapsedMs

Exemplo:

_logger.LogInformation(
"Game {GameId} published by {UserId}",
gameId,
userId);

---

# 10. Observability Rules

Sempre instrumentar:

- APIs
- Banco
- Redis
- Jobs
- Storage

Via OpenTelemetry.

Coletar:

- Latência
- Throughput
- Errors
- Availability

---

# 11. Testing Rules

Nenhuma funcionalidade relevante sem testes.

Prioridades:

## Unit Tests

Domínio

## Integration Tests

Application Services

## API Tests

Controllers

---

Cobertura mínima:

80%

---

Frameworks:

- xUnit
- FluentAssertions
- Moq

---

# 12. Performance Rules

Evitar:

SELECT *

N+1 Queries

Materialização excessiva

Loops em coleções gigantes

---

Utilizar:

Pagination

Async/Await

CancellationToken

Cache Redis

Projection DTO

---

# 13. Frontend Rules

Arquitetura:

app/

core
shared
public
player
developer
admin

---

Componentes:

Responsabilidade única.

Evitar:

- component gigantes
- lógica excessiva em template

---

Utilizar:

RxJS
Services
Resolvers
Guards

---

# 14. Gameplay SDK Rules

Todo jogo deve comunicar:

gameLoadingFinished

gameplayStart

gameplayStop

commercialBreak

rewardedBreak

captureError

measure

---

Nunca acoplar o jogo ao backend.

Sempre utilizar:

GameplayBridgeService

---

# 15. Leaderboards

Armazenamento:

Redis Sorted Sets

Banco:

Snapshots periódicos

Regras:

- Score maior primeiro
- Atualizações idempotentes
- Ranking consultável

---

# 16. Build Pipeline

Pipeline mínimo:

Restore

Build

Test

Lint

Package

Docker Build

Security Scan

Publish

---

Falha em qualquer etapa:

Interromper execução

Não continuar deploy

Registrar erro

---

# 17. Documentation Rules

Toda feature deve gerar:

- Architecture Notes
- Sequence Flow
- Decisions
- Limitations

Arquivos:

docs/

---

# 18. Agent Behavior

Sempre:

✅ Propor plano antes de alterar estruturas grandes

✅ Explicar impactos

✅ Criar documentação

✅ Criar testes

✅ Registrar decisões arquiteturais

✅ Manter compatibilidade EAF

✅ Preservar padrões ABP

---

Nunca:

❌ Quebrar camadas

❌ Ignorar testes

❌ Ignorar logging

❌ Ignorar segurança

❌ Adicionar dependências sem justificativa

❌ Duplicar lógica

---

# 19. Definition Of Done

Uma tarefa só está concluída quando:

- Código implementado
- Testes implementados
- Build executado
- Documentação criada
- Logs válidos
- Pipeline válida
- Sem warnings críticos
- Sem secrets expostos

---

# 21. Tools e Recursos Disponíveis

O agente deve utilizar as ferramentas e recursos abaixo ao operar neste repositório.

## Devin Tools Nativas
- `exec`, `read`, `write`, `edit`, `MultiEdit`, `grep`, `find_file_by_name`.
- `skill` (ativar skills reutilizáveis), `todo_write` (gerenciar tarefas), `message_user` (comunicar com o usuário).
- `git_*` (operações de PR, merge, comentários, labels), `deploy` (frontend/backend).
- `mcp_tool` (acesso a MCP servers), `devin_mcp` / `devin_docs` (gestão do ambiente Devin).
- `web_search` / `web_get_contents` (pesquisa web), `upload_attachment` / `download_attachment`.

## MCP Servers Disponíveis
- `deepwiki`: documentação de repositórios GitHub.
- `firecrawl`: extração de conteúdo web em escala.
- `microsoft-learn`: documentação Microsoft oficial.
- `monday`: gestão de boards e itens.
- `notion`: gestão de páginas e databases.
- `sonarqube`: análise de qualidade de código.
- `tavily`: busca web em tempo real.

## Skills Relevantes
- ABP/EAF: `abp-core`, `abp-ef-core`, `abp-angular`, `abp-testing`, `abp-multi-tenancy`, `abp-application-layer`, `abp-ddd`, `abp-development-flow`.
- .NET/infra: `dotnet-github-actions`, `aspnet-core-api`, `entity-framework-core`, `postgresql-optimization`, `security-jwt`, `modern-csharp-coding-standards`.
- Agent workflow: `create-agent-harness`, `writing-skills`, `harness-repo-structure`, `verification-before-completion`, `systematic-debugging`, `receiving-code-review`.
- Comunicação: `caveman`, `caveman-commit`, `caveman-review`.

## Secrets Configurados
- `FIRECRAWL_API_KEY`, `GITHUB_PAT`, `OMNIROUTE_API_KEY`, `SONARQUBE_TOKEN`.

## Repositório Central
- `afonsoft/agents-skills` — source of truth para skills, rules e conhecimento reutilizável.

---

# 22. Final Goal

Construir uma plataforma de jogos enterprise-grade:

- Escalável
- Modular
- Cloud Native
- Observável
- Testável
- Segura
- Baseada em EAF/ABP
- Preparada para milhões de acessos
- Preparada para múltiplos desenvolvedores
- Preparada para monetização futura
