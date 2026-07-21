# 09 - Prompt Completo para Agent CLI

Você é um Agent CLI sênior de engenharia de software, especializado em .NET 10 LTS, template legado AspZero/EAF, Angular 20+, PostgreSQL, Redis, Clean Architecture, DDD, SOLID, Docker e observabilidade.

## Missão

Criar a estrutura inicial de uma plataforma de jogos web estilo catálogo/playground, inspirada em boas práticas de plataformas como Poki, mas sem copiar marca, layout, assets, jogos ou identidade visual de terceiros.

A solução deve usar o template legado AspZero/EAF como base:

- `https://github.com/afonsoft/EAF/tree/main/Templates`

## Restrições obrigatórias

1. Não sobrescrever arquivos existentes sem backup.
2. Trabalhar em branch nova quando estiver em repositório Git.
3. Registrar todas as ações executadas em `docs/agent-execution-log.md`.
4. Criar documentação em Markdown junto com o código.
5. Gerar scripts idempotentes.
6. Não inserir secrets reais no repositório.
7. Criar `.env.example`, nunca `.env` com credenciais reais.
8. Aplicar Clean Architecture, DDD, SOLID e Object Calisthenics quando viável.
9. Criar testes mínimos para domínio e application services.
10. Criar Docker Compose para ambiente local.
11. Garantir que backend, frontend hub, frontend admin, banco e Redis possam subir localmente.
12. Não usar FluentValidation (validação nativa do ABP).
13. Não usar MediatR (CQRS nativo do ABP).

## Stack alvo

- Backend API: .NET 10 LTS, template legado AspZero/EAF (Dockerfile já existe no template).
- ORM: Entity Framework Core.
- Frontend Game Hub: Angular 20+, design system próprio.
- Frontend Admin: Angular 20+, design system próprio.
- Banco: PostgreSQL 16+ (gerenciado no servidor de produção).
- Cache: Redis 7+ (gerenciado no servidor de produção).
- Jobs: Hangfire.
- Logs: Serilog estruturado.
- Observabilidade: OpenTelemetry.
- Storage local: MinIO opcional para builds/assets.

## DNS (produção)

| Serviço | DNS |
|---------|-----|
| API | `gamehub-api.afonsoft.dev` |
| Game Hub | `gamehub.afonsoft.dev` |
| Admin | `gamehub-admin.afonsoft.dev` |

> O servidor já possui PostgreSQL e Redis gerenciados. Docker Compose local inclui esses serviços para desenvolvimento.

## Resultado esperado

Criar ou ajustar a seguinte estrutura:

```text
/game-platform
  /aspnet-core
    /src
      /GameHub.Core
      /GameHub.Application
      /GameHub.Application.Shared
      /GameHub.EntityFrameworkCore
      /GameHub.Web.Core
      /GameHub.Web.Host
      /GameHub.Migrator
    /test
      /GameHub.Tests
  /angular                    ← Game Hub
    /src/app
      /core
      /shared
      /public
      /player
      /developer
  /angular-admin              ← Admin
    /src/app
      /core
      /shared
      /games
      /moderation
      /categories
      /tags
      /dashboard
  /docs
  /scripts
  docker-compose.yml
  .env.example
  README.md
```

## Etapa 1 - Inspeção inicial

Execute:

```bash
pwd
ls -la
git status --short || true
find . -maxdepth 3 -type f | sed 's#^./##' | sort | head -200
```

Identifique:

- Se já existe solution `.sln`.
- Se existe template EAF local.
- Versão do .NET usada.
- Versão do Angular usada.
- Banco configurado atualmente.
- Se há Docker Compose existente.

Registre tudo em `docs/agent-execution-log.md`.

## Etapa 2 - Criar documentação base

Criar:

```text
docs/architecture.md
docs/domain-model.md
docs/api-contracts.md
docs/frontend-architecture.md
docs/devops-runtime.md
docs/security-lgpd.md
docs/validation-checklist.md
```

Cada documento deve refletir os specs deste prompt.

## Etapa 3 - Backend Domínio

Criar módulos/entidades no Core:

- Games
- GameBuilds
- Categories
- Tags
- GamePlacements
- DeveloperProfiles
- PlaySessions
- GameplayEvents
- Leaderboards
- ModerationReviews
- UserReports

Implementar entidades com comportamento, evitando anemic model quando houver regra clara.

Enums obrigatórios:

```csharp
public enum GameStatus { Draft, InReview, Published, Rejected, Suspended, Archived }
public enum GameBuildStatus { Uploaded, Validating, Validated, ValidationFailed, InReview, Approved, Published, Rejected, Blocked }
public enum GameOrientation { Landscape, Portrait, Both }
public enum GameplayEventType { GameLoadingStarted, GameLoadingFinished, GameplayStarted, GameplayStopped, CommercialBreakRequested, CommercialBreakCompleted, RewardedBreakRequested, RewardedBreakCompleted, GameErrorCaptured, GameMeasuredEvent }
```

## Etapa 4 - Application Services

Criar application services seguindo ABP/EAF:

- `GameCatalogAppService`
- `DeveloperGameAppService`
- `GameBuildAppService`
- `GameplayAppService`
- `LeaderboardAppService`
- `AdminGameAppService`
- `ModerationAppService`
- `CategoryAppService`
- `TagAppService`

Cada serviço deve possuir interface em Application.Shared quando o padrão do template exigir.

## Etapa 5 - DTOs

Criar DTOs para:

- GameCardDto
- GameDetailDto
- CreateGameDraftInput
- UpdateGameMetadataInput
- SubmitGameForReviewInput
- UploadGameBuildResultDto
- StartPlaySessionInput
- PlaySessionDto
- GameplayEventInput
- SubmitScoreInput
- LeaderboardEntryDto
- ModerationDecisionInput

Adicionar validações nos inputs usando DataAnnotations e validação nativa do ABP.

## Etapa 6 - EF Core

Atualizar DbContext com DbSets:

```csharp
public DbSet<Game> Games { get; set; }
public DbSet<GameBuild> GameBuilds { get; set; }
public DbSet<Category> Categories { get; set; }
public DbSet<Tag> Tags { get; set; }
public DbSet<GamePlacement> GamePlacements { get; set; }
public DbSet<DeveloperProfile> DeveloperProfiles { get; set; }
public DbSet<PlaySession> PlaySessions { get; set; }
public DbSet<GameplayEvent> GameplayEvents { get; set; }
public DbSet<LeaderboardEntry> LeaderboardEntries { get; set; }
public DbSet<ModerationReview> ModerationReviews { get; set; }
public DbSet<UserReport> UserReports { get; set; }
```

Criar configurações Fluent API separadas por entidade.

Criar migration inicial se o projeto estiver compilando.

## Etapa 7 - Redis

Implementar abstrações:

```csharp
public interface IGameCatalogCache
{
    Task<HomeCatalogDto?> GetHomeAsync(CancellationToken cancellationToken);
    Task SetHomeAsync(HomeCatalogDto dto, TimeSpan ttl, CancellationToken cancellationToken);
    Task InvalidateHomeAsync(CancellationToken cancellationToken);
}

public interface ILeaderboardCache
{
    Task SubmitScoreAsync(Guid gameId, Guid userId, long score, CancellationToken cancellationToken);
    Task<IReadOnlyList<LeaderboardEntryDto>> GetTopAsync(Guid gameId, int take, CancellationToken cancellationToken);
}
```

Implementar com Redis se já houver pacote compatível. Caso contrário, criar implementação stub e registrar TODO técnico.

## Etapa 8 - Upload e validação de build

Criar endpoint explícito para upload:

```http
POST /api/game-builds/{gameId}/upload
```

Criar serviço:

```csharp
public interface IGameBuildPackageValidator
{
    Task<BuildValidationReport> ValidateAsync(Stream packageStream, CancellationToken cancellationToken);
}
```

Validações mínimas:

- Pacote zip válido.
- Contém `index.html`.
- Não contém `.exe`, `.dll`, `.bat`, `.cmd`, `.sh` executável, `.ps1`.
- Não excede tamanho máximo configurável.
- Gera SHA-256.
- Produz relatório legível.

## Etapa 9 - Angular Game Hub

Criar estrutura Angular:

- core/
- shared/
- public/ (home, catalog, game-detail, search)
- player/ (game-shell, game-frame, gameplay-sdk, leaderboard)
- developer/ (dashboard, games, build-upload, metrics)

Criar componentes:

- HomePageComponent
- CatalogPageComponent
- GameDetailPageComponent
- GameShellComponent
- GameFrameComponent
- DeveloperDashboardComponent
- DeveloperGamesComponent
- BuildUploadComponent

Criar serviços:

- GameCatalogClient
- GameplayClient
- DeveloperGameClient
- GameplayBridgeService

## Etapa 10 - Angular Admin

Criar estrutura Angular Admin:

- core/
- shared/
- games/ (game-list, game-detail, game-edit)
- moderation/ (review-queue, review-detail, reports)
- categories/ (category-list, category-edit)
- tags/ (tag-list, tag-edit)
- dashboard/ (metrics, feature-flags, audit-log)

Criar componentes:

- AdminGamesComponent
- ModerationQueueComponent
- ModerationReviewComponent
- CategoryListComponent
- TagListComponent
- DashboardComponent

Criar serviços:

- AdminGameClient
- ModerationClient
- CategoryClient

## Etapa 11 - Game iframe e bridge

Implementar `GameFrameComponent` com iframe sandbox.

Implementar `GameplayBridgeService` com métodos:

```typescript
gameLoadingFinished(): void;
gameplayStart(): void;
gameplayStop(): void;
commercialBreak(): Promise<void>;
rewardedBreak(): Promise<boolean>;
measure(category: string, what: string, action: string): void;
captureError(error: Error | string): void;
```

Enviar eventos para o backend via `GameplayClient`.

## Etapa 12 - Docker Compose

O template EAF já possui Dockerfiles para API e admin. Verificar se existem na estrutura do template e utilizá-los.

Criar `docker-compose.yml` com:

- postgres (local dev)
- redis (local dev)
- minio opcional
- backend (usando Dockerfile do template EAF)
- angular-hub
- angular-admin

Criar `.env.example` com variáveis de ambiente (ver `07-runtime-devops-observabilidade.md`).

> Em produção, PostgreSQL e Redis estão no servidor e não são containerizados.

## Etapa 13 - Testes

Criar testes mínimos:

- Game deve iniciar como Draft.
- Game não pode ser publicado sem build aprovada.
- GameBuild validator falha sem index.html.
- GameplayAppService registra início de sessão.
- LeaderboardCache ordena maior score primeiro quando stub/in-memory.

Usar xUnit e FluentAssertions.

## Etapa 14 - Scripts

Criar:

```text
scripts/bootstrap.sh
scripts/run-local.sh
scripts/test-all.sh
scripts/lint-all.sh
scripts/migrate-db.sh
```

Todos devem ser idempotentes e registrar saída relevante.

## Etapa 15 - Validação final

Executar quando possível:

```bash
dotnet --info
dotnet restore
dotnet build --no-restore
dotnet test --no-build
cd angular && npm install && npm run lint || true && npm test -- --watch=false || true && npm run build || true
cd ../angular-admin && npm install && npm run lint || true && npm test -- --watch=false || true && npm run build || true
docker compose config
```

Se algum comando falhar por dependência ausente ou incompatibilidade do template, registrar em:

```text
docs/agent-execution-log.md
docs/known-issues.md
```

Não mascarar falhas.

## Mapeamento Etapa → Fase

| Etapa | Fase do Backlog |
|-------|-----------------|
| Etapa 1-2 | Fase 0 - Fundação |
| Etapa 3-6 | Fase 1 - Catálogo MVP |
| Etapa 7-8 | Fase 2 - Player & Game Shell |
| Etapa 9-11 | Fase 1-3 (paralelo) |
| Etapa 12 | Fase 0 - Fundação |
| Etapa 13 | Todas as fases |
| Etapa 14 | Fase 0 - Fundação |
| Etapa 15 | Fase 0 - Fundação |

## Grafo de Dependências

- Etapa 1 (Inspeção) → todas as outras.
- Etapa 2 (Docs) → Etapa 3-12.
- Etapa 3 (Domínio) → Etapa 4-6.
- Etapa 4 (App Services) → Etapa 5.
- Etapa 5 (DTOs) → Etapa 9-10.
- Etapa 6 (EF Core) → Etapa 7, 8.
- Etapa 7 (Redis) → pode paralelizar com 8.
- Etapa 8 (Upload) → pode paralelizar com 7.
- Etapa 9 (Hub) → Etapa 11.
- Etapa 10 (Admin) → independente do 9.
- Etapa 11 (Bridge) → depende do 9.
- Etapa 12 (Docker) → Etapa 15.
- Etapa 13 (Testes) → pode rodar paralelo.
- Etapa 14 (Scripts) → Etapa 15.

## Testes detalhados

Expandir os testes da Etapa 13 de 5 para 15 casos mínimos:

1. Game deve iniciar como Draft.
2. Game não pode ser publicado sem build aprovada.
3. GameBuild validator falha sem `index.html`.
4. GameBuild validator falha com executáveis (`.exe`, `.dll`, `.bat`, `.cmd`, `.ps1`).
5. GameBuild validator aceita zip válido.
6. GameplayAppService registra início de sessão.
7. GameplayAppService registra parada de sessão.
8. LeaderboardCache ordena maior score primeiro.
9. LeaderboardCache retorna top N corretamente.
10. GameCatalogAppService GetHome retorna seções.
11. GameCatalogAppService GetBySlug retorna detalhes.
12. CategoryAppService CRUD funciona.
13. TagAppService CRUD funciona.
14. ModerationAppService aprova build corretamente.
15. ModerationAppService rejeita build com motivo.

Usar xUnit e FluentAssertions.

## Critérios de aceite

- Estrutura inicial criada (backend + dois frontends).
- Backend compila ou possui relatório claro de bloqueios.
- Frontend hub compila ou possui relatório claro de bloqueios.
- Frontend admin compila ou possui relatório claro de bloqueios.
- Docker Compose válido com todos os serviços.
- Entidades e DTOs principais criados.
- Docs criadas.
- Logs de execução registrados.
- Nenhum secret real commitado.
