# GameHub - Plataforma de Jogos Web

Plataforma enterprise-grade de catálogo, execução e gestão de jogos HTML5/WebGL, inspirada em portais modernos de distribuição de jogos.

> **Objetivo**: criar uma plataforma própria. Não copiar marca, layout, conteúdo, jogos, assets ou experiência proprietária de terceiros.

---

## Stack Tecnológica

| Camada | Tecnologias |
|---|---|
| **Backend API** | .NET 10 LTS, ASP.NET Core, template legado AspZero/EAF, EF Core, AutoMapper, Hangfire |
| **Frontend Game Hub** | Angular 20+, TypeScript strict, RxJS, design system próprio |
| **Frontend Admin** | Angular 20+, TypeScript strict, RxJS, design system próprio |
| **Banco** | PostgreSQL 16+ (preferencial) ou SQL Server 2022+ |
| **Cache** | Redis 7+ (catálogo, rate limiting, leaderboards via Sorted Sets, locks distribuídos) |
| **Storage** | S3/MinIO/Azure Blob (game-builds, thumbnails, screenshots) |
| **Observabilidade** | Serilog (logs JSON), OpenTelemetry (traces + métricas), CorrelationId |
| **Container** | Docker, Docker Compose |
| **Segurança** | JWT/OIDC, RBAC (ABP), CSP, iframe sandbox, MFA (admin/moderator), CORS, LGPD |
| **Testes** | xUnit, FluentAssertions |

> **Não usar**: FluentValidation (validação nativa do ABP), MediatR (CQRS nativo do ABP).

---

## Arquitetura de Frontends

A plataforma possui **duas aplicações Angular independentes** consumindo a mesma API backend:

| App | Diretório | DNS (prod) | Escopo | Porta (dev) |
|-----|-----------|------------|--------|-------------|
| Game Hub | `angular/` | `gamehub.afonsoft.dev` | Catálogo, jogos, busca, perfil dev | 4200 |
| Admin | `angular-admin/` | `gamehub-admin.afonsoft.dev` | Gestão, moderação, métricas, config | 4201 |
| API | `aspnet-core/` | `gamehub-api.afonsoft.dev` | Backend REST | 5000 |

**Infraestrutura**: PostgreSQL e Redis já gerenciados no servidor de produção. O template EAF já possui Dockerfiles para API e admin.

---

## Funcionalidades

### Personas

- **Jogador Anônimo**: navega, busca, joga, vota, denuncia
- **Jogador Autenticado**: perfil, favoritos, save na nuvem, leaderboards, recomendações
- **Desenvolvedor**: perfil, submete jogos, upload de builds, métricas
- **Moderador**: revisa, aprova/rejeita, trata denúncias
- **Admin**: usuários, roles, feature flags, métricas, auditoria

### Funcionalidades Públicas (Game Hub)

- Home com seções: destaques, novos, mais jogados, tendências, recomendados, categorias
- Página de detalhe do jogo com execução em iframe sandbox
- Busca com filtros: categoria, tag, dispositivo, orientação
- Leaderboards com Redis Sorted Sets

### Gameplay SDK / Bridge

10 eventos comunicados do iframe para a plataforma:

| Evento | Descrição |
|--------|-----------|
| `GameLoadingStarted` | Início do carregamento |
| `GameLoadingFinished` | Carregamento concluído |
| `GameplayStarted` | Jogador iniciou a sessão |
| `GameplayStopped` | Jogador pausou/saiu |
| `CommercialBreakRequested` | Break comercial solicitado |
| `CommercialBreakCompleted` | Break comercial finalizado |
| `RewardedBreakRequested` | Rewarded ad solicitado |
| `RewardedBreakCompleted` | Rewarded ad finalizado |
| `GameErrorCaptured` | Erro capturado |
| `GameMeasuredEvent` | Evento de medição |

### Portal do Desenvolvedor (Game Hub)

- Wizard de submissão em 5 etapas
- Upload de builds HTML5/WebGL (zip/tar) com validação obrigatória
- Validações: `index.html` obrigatório, tamanho máximo (100MB), SHA-256, sem executáveis (.exe, .dll, .bat, .cmd, .ps1)
- Versionamento imutável (semver), publicação no CDN após aprovação

### Moderação & Publicação (Admin)

- Fila de revisão com aprovação/rejeição
- Workflow de suspensão e denúncias
- Histórico de moderação auditável

### Monetização (Preparado)

- Interface de provedor de ads (`IAdProvider`)
- Revenue share com desenvolvedores
- Provider nulo/fake inicialmente

### Localização (i18n)

- pt-BR e en-US para MVP
- Backend: `IStringLocalizer` do ABP
- Frontend: `@angular/localize` ou similar
- Título, descrição, instruções, changelog, tags customizadas localizáveis

---

## Arquitetura

**Modular Monolith** com **Clean Architecture + DDD + Template legado AspZero/EAF**

### Camadas

```
aspnet-core/src/
  GameHub.Core              → Domínio (entidades, value objects, repositórios)
  GameHub.Application       → Casos de uso (Application Services)
  GameHub.Application.Shared → DTOs, Permissions, FeatureFlags
  GameHub.EntityFrameworkCore → DbContext, Migrations, Configurations
  GameHub.Web.Core          → Middleware, Filters, Security
  GameHub.Web.Host          → Controllers, Startup
  GameHub.Migrator          → Runner de migrações
aspnet-core/test/
  GameHub.Tests             → Testes xUnit
angular/                    → Game Hub
angular-admin/              → Admin
```

### Bounded Contexts

1. **Catalog** — Game, Category, Tag, GamePlacement
2. **Build Management** — GameBuild, BuildValidationReport
3. **Gameplay Analytics** — PlaySession, GameplayEvent, GameMetricSnapshot
4. **Developer Portal** — DeveloperProfile, GameSubmission
5. **Moderation** — ModerationReview, UserReport
6. **Monetization** — AdPlacement, RevenueShareStatement

### Regras de Dependência

```
Core ← Application ← Infrastructure
Web  ← Application
```

Jamais: Core → Infrastructure, Core → Web, Application → Web.

---

## API

**Base path**: `/api/services/app` (ABP Dynamic API)

### Auth

| Endpoint | Descrição |
|----------|-----------|
| `POST /api/TokenAuth/Authenticate` | Login, retorna JWT |
| `POST /api/services/app/Account/Register` | Registro |

### Endpoints Públicos (Game Hub)

| Grupo | Endpoints |
|---|---|
| GameCatalog | GetHome, GetGames, GetBySlug, Search, GetRelated |
| Gameplay | StartSession, FinishLoading, Start, Stop, CaptureError, Measure |
| Leaderboard | SubmitScore, GetTop |

### Endpoints Desenvolvedor (Game Hub)

| Grupo | Endpoints |
|---|-----------|
| DeveloperProfile | CreateOrUpdate |
| DeveloperGame | GetMyGames, CreateDraft, SubmitForReview, GetBuilds |
| Upload | `POST /api/game-builds/{gameId}/upload` |

### Endpoints Admin

| Grupo | Endpoints |
|---|---|
| AdminGame | GetAll, ApproveBuild, RejectBuild, Publish, Suspend |
| Category/Tag | CreateOrUpdate |
| Moderation | GetPendingReviews, CompleteReview |

### Health Check

```
GET /health
```

**Response envelope**: `{ "result": T, "success": true, "error": null }` (padrão ABP)

**Error codes**: 400 (validação), 403 (permissão), 404 (não encontrado), 422 (regra de negócio)

---

## Frontend Game Hub (`angular/`)

### Módulos

```
core/       → auth, http, guards, interceptors, telemetry
shared/     → componentes, pipes, directives, models (design system próprio)
public/     → home, catalog, game-detail, search
player/     → game-shell, game-frame, gameplay-sdk, leaderboard
developer/  → dashboard, games, build-upload, metrics
```

### GameShellComponent

- Carrega dados do jogo por slug
- Cria sessão de gameplay
- Renderiza iframe com sandbox: `allow-scripts allow-pointer-lock allow-same-origin allow-forms`
- `referrerpolicy="no-referrer"`, `allow="fullscreen gamepad"`
- Injeta JS wrapper, coleta eventos, encerra sessão no exit

### UX

- Cards responsivos, lazy loading nativo (`loading="lazy"`), skeleton loading
- Paginação controlada (MVP), busca com debounce (300ms)
- Filtros persistidos na URL, acessibilidade por teclado
- Estado: services + RxJS; avaliar Angular Signals depois

---

## Frontend Admin (`angular-admin/`)

### Módulos

```
core/          → auth, http, guards, interceptors
shared/        → componentes reutilizados do game hub
games/         → game-list, game-detail, game-edit
moderation/    → review-queue, review-detail, reports
categories/    → category-list, category-edit
tags/          → tag-list, tag-edit
dashboard/     → metrics, feature-flags, audit-log
```

---

## DevOps & Observabilidade

### Docker Compose (local)

Serviços: `postgres`, `redis`, `minio`, `backend` (porta 5000), `angular-hub` (porta 4200), `angular-admin` (porta 4201)

> Em produção, PostgreSQL e Redis estão no servidor e não são containerizados. O template EAF já possui Dockerfiles para API e admin.

### DNS (produção)

| Serviço | DNS |
|---------|-----|
| API | `gamehub-api.afonsoft.dev` |
| Game Hub | `gamehub.afonsoft.dev` |
| Admin | `gamehub-admin.afonsoft.dev` |

### Scripts

```
scripts/bootstrap.sh        → Setup inicial
scripts/run-local.sh        → Rodar local
scripts/test-all.sh         → Testar tudo
scripts/lint-all.sh         → Lint tudo
scripts/migrate-db.sh       → Migrações
scripts/seed-dev.sh         → Dados de desenvolvimento
```

### Pipeline CI/CD

Restore → Build → Test → Lint → Docker Build → Security Scan → Publish

### Logs Estruturados (Serilog)

Campos: `correlationId`, `tenantId`, `userId`, `gameId`, `buildId`, `requestPath`, `elapsedMs`

### Métricas

- Requests/endpoint, p95/p99 latency, errors
- Uploads, validações, gameplay events/game
- Avg session time, loading-finished rate

### Cache TTL (Redis)

| Dado | TTL |
|------|-----|
| Home catalog | 5 min |
| Categorias/Tags | 30 min |
| Game detail | 10 min |
| Leaderboard | 1 min |
| Search results | 2 min |

### Hangfire Jobs

- ValidateGameBuildJob
- PublishGameBuildJob
- AggregateGameplayMetricsJob
- RecalculateTrendingGamesJob
- CleanupExpiredUploadsJob
- SyncRedisLeaderboardSnapshotJob

---

## Segurança & LGPD

- JWT/OIDC com RBAC (4 roles: Player, Developer, Moderator, Admin)
- MFA para Admin/Moderator
- CORS configurado para dois frontends (`gamehub.afonsoft.dev`, `gamehub-admin.afonsoft.dev`)
- Iframe sandbox isolado para jogos
- CSP restritiva, origin isolada para jogos (`games.afonsoft.dev`)
- Validação de builds: sem executáveis, sem scripts externos não autorizados
- LGPD: minimizar dados pessoais, permitir exclusão/exportação de dados
- Auditoria para todas as ações admin
- Supply chain: Dependabot/Renovate, scanning de vulnerabilidades

---

## Estrutura do Repositório

```
game-platform/
├── aspnet-core/
│   ├── src/
│   │   ├── GameHub.Core/
│   │   ├── GameHub.Application/
│   │   ├── GameHub.Application.Shared/
│   │   ├── GameHub.EntityFrameworkCore/
│   │   ├── GameHub.Web.Core/
│   │   └── GameHub.Web.Host/
│   ├── test/
│   │   └── GameHub.Tests/
│   └── GameHub.Migrator/
├── angular/                    ← Game Hub (porta 4200)
├── angular-admin/              ← Admin (porta 4201)
├── docs/
├── scripts/
└── .specs/                     ← Especificações detalhadas
```

---

## Convenções

| Elemento | Padrão |
|---|---|
| Entidades | PascalCase (`Game`, `GameBuild`) |
| Enums | PascalCase (`GameStatus.Draft`) |
| Application Services | `*AppService` |
| DTOs output | `*Dto` |
| DTOs input | `*Input` |
| Interfaces | `I*` |
| Aggregate Root | `FullAuditedAggregateRoot<Guid>` |
| EF Config | `IEntityTypeConfiguration<T>` |
| Angular Services | `*Client` |
| Branches | `feature/*`, `bugfix/*`, `hotfix/*`, `refactor/*` |
| Commits | Conventional Commits |
| Builds | semver (`1.0.0`) |

---

## Backlog Resumido

| Fase | Escopo |
|---|---|
| **0 - Foundation** | Repo, template AspZero/EAF, Docker Compose (backend + 2 frontends), docs, logs |
| **1 - Public Catalog MVP** | Game entity, categorias/tags, home page, detail page, busca full-text, seed data |
| **2 - Player & Game Shell** | iframe sandbox, PlaySession, 10 gameplay events, leaderboard básico |
| **3 - Developer Portal** | Perfil dev, draft game, upload, validação zip, status |
| **4 - Moderation & Publishing** | Fila de revisão (admin), aprovação/rejeição, publish/suspend, reports |
| **5 - Observability & Analytics** | Métricas agregadas, Hangfire jobs, dashboard admin, trending |
| **6 - Hardening** | CSP avançado, origin isolada, scanning builds, LGPD data export/delete |
| **7 - Monetization** | Interface IAdProvider, commercial break, rewarded break, revenue reports |

---

## Specs Detalhadas

Os arquivos em `.specs/` contêm as especificações completas do projeto:

| # | Arquivo | Descrição |
|---|---------|-----------|
| 00 | `00-contexto-fontes.md` | Contexto, referências, padrões adotados/rejeitados, fora do escopo |
| 01 | `01-requisitos-tecnicos.md` | Stack, runtimes, versões de pacotes, NFRs |
| 02 | `02-funcionalidades-produto.md` | Personas, features, critérios de aceite |
| 03 | `03-arquitetura-clean-ddd-abp-eaf.md` | Clean Architecture, DDD, bounded contexts, comunicação entre contextos |
| 04 | `04-modelagem-dados.md` | Entidades com tipos C#, FKs, Value Objects, enums, constraints |
| 05 | `05-api-contratos.md` | Endpoints, DTOs, error format, rate limiting, upload contract |
| 06 | `06-frontend-angular.md` | Game Hub: rotas, guards, interceptors, GameplayBridge, design tokens |
| 06b | `06b-frontend-admin.md` | Admin: rotas, guards, componentes, interceptors, state |
| 07 | `07-runtime-devops-observabilidade.md` | Docker, CI/CD (GitHub Actions), logs, métricas, traces, cache TTL |
| 08 | `08-seguranca-lgpd-compliance.md` | JWT, RBAC matrix, CSP, LGPD, CORS |
| 09 | `09-prompt-agent-cli.md` | Prompt executável com 15 etapas, mapeamento fase-dependência, 15 testes |
| 10 | `10-checklist-validacao.md` | Checklist completo: repo, backend, frontends, devops, segurança, testes |
| 11 | `11-backlog-sprints.md` | 8 fases com cadência, dependências, estimativas, corte MVP |
| 12 | `12-rbac-permissions.md` | Constantes de permissão, matriz role-permissão, AuthorizationProvider |
| 13 | `13-frontend-routing.md` | Rotas lazy-loaded para ambos apps, guards, resolvers |
| 14 | `14-dto-complete-reference.md` | Referência completa de todos os DTOs com C# |
| 15 | `15-csp-security-headers.md` | CSP directives, security headers, iframe security, rate limiting impl |

Para executar via Agent CLI, copie o conteúdo de `.specs/09-prompt-agent-cli.md` e execute em um repositório vazio ou branch isolada. O agente deve gerar a estrutura completa do projeto (backend + dois frontends), docs, Docker Compose, domínio, contratos de API, testes e scripts de validação.
