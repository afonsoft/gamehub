# 03 - Arquitetura Clean Architecture, DDD, AspZero/EAF

## Visão arquitetural

Usar monólito modular com fronteiras claras por contexto. A base AspZero/EAF já oferece recursos de DI, repositórios, Unit of Work, autorização, validação, auditoria, logging, localization e dynamic API. Preservar esses padrões evitando criar abstrações paralelas desnecessárias.

**Não usar**: FluentValidation (validação nativa do ABP), MediatR (CQRS nativo do ABP).

## Estrutura do repositório

```text
game-platform/
  aspnet-core/
    src/
      GameHub.Core/
        Games/
        Builds/
        Catalog/
        Gameplay/
        Moderation/
        Developers/
        Monetization/
        SharedKernel/
      GameHub.Application/
        Games/
        Builds/
        Catalog/
        Gameplay/
        Moderation/
        Developers/
        Monetization/
      GameHub.Application.Shared/
        Dtos/
        Permissions/
        FeatureFlags/
      GameHub.EntityFrameworkCore/
        Migrations/
        Configurations/
        Repositories/
      GameHub.Web.Core/
        Middleware/
        Filters/
        Security/
      GameHub.Web.Host/
        Controllers/
        Startup/
        appsettings.json
      GameHub.Migrator/
    test/
      GameHub.Tests/
  angular/                    ← Game Hub (catálogo, jogos, busca)
    src/app/
      core/
      shared/
      public/
      player/
      developer/
  angular-admin/              ← Admin (gestão, moderação, métricas)
    src/app/
      core/
      shared/
      games/
      moderation/
      categories/
      reports/
      dashboard/
  docs/
  scripts/
  docker-compose.yml
  .env.example
```

### Bounded contexts

#### Catalog

Responsável por busca, listagem, home, categorias, tags e recomendações simples.

Agregados:

- Game
- Category
- Tag
- GamePlacement (posicionamento na home: featured, carousel, trending)

#### Build Management

Responsável por upload, validação, versionamento e publicação de builds.

Agregados:

- GameBuild
- BuildValidationReport

#### Gameplay Analytics

Responsável por eventos de gameplay e agregações.

Agregados:

- PlaySession
- GameplayEvent
- GameMetricSnapshot

#### Developer Portal

Responsável por perfil do desenvolvedor, submissions e status.

Agregados:

- DeveloperProfile
- GameSubmission

#### Moderation

Responsável por revisões, reports e decisões.

Agregados:

- ModerationReview
- UserReport

#### Monetization

Responsável por abstração de ads/revenue share.

Agregados:

- AdPlacement
- RevenueShareStatement

## Princípios SOLID aplicados

### SRP

Cada Application Service deve orquestrar um caso de uso, não misturar persistência, upload, validação, publicação e notificação no mesmo método.

### OCP

Providers externos devem ser plugáveis:

```csharp
public interface IGameAssetStorage
{
    Task<StoredAsset> StoreAsync(GameBuildPackage package, CancellationToken cancellationToken);
}

public interface IAdProvider
{
    Task<AdBreakResult> RequestCommercialBreakAsync(AdBreakRequest request, CancellationToken cancellationToken);
}
```

### LSP

Implementações de storage/ads/analytics devem respeitar contratos sem lançar exceções inesperadas para cenários suportados.

### ISP

Evitar interfaces gigantes como `IGameService`. Separar:

- IGameCatalogAppService
- IGameSubmissionAppService
- IGamePlayerAppService
- IGameAdminAppService

### DIP

Domínio e application dependem de abstrações, infraestrutura implementa.

## Object Calisthenics sugerido

- Evitar getters/setters públicos indiscriminados em entidades de domínio.
- Preferir Value Objects para Slug, AgeRating, GameOrientation, BuildVersion.
- Não usar else excessivo em regras de validação.
- Encapsular coleções.
- Métodos pequenos e com nomes de intenção.

## Exemplo de entidade de domínio

```csharp
public sealed class Game : FullAuditedAggregateRoot<Guid>
{
    private readonly List<GameBuild> _builds = new();

    public string Title { get; private set; }
    public string Slug { get; private set; }
    public GameStatus Status { get; private set; }
    public IReadOnlyCollection<GameBuild> Builds => _builds.AsReadOnly();

    private Game() { }

    public Game(Guid id, string title, string slug)
    {
        Id = id;
        Rename(title);
        ChangeSlug(slug);
        Status = GameStatus.Draft;
    }

    public void SubmitForReview()
    {
        if (!_builds.Any(b => b.Status == GameBuildStatus.Validated))
            throw new UserFriendlyException("É necessário possuir ao menos uma build validada.");

        Status = GameStatus.InReview;
    }

    public void Publish(GameBuild build)
    {
        if (build.Status != GameBuildStatus.Approved)
            throw new UserFriendlyException("A build precisa estar aprovada para publicação.");

        Status = GameStatus.Published;
        build.Publish();
    }

    private void Rename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new UserFriendlyException("Título do jogo é obrigatório.");

        Title = title.Trim();
    }

    private void ChangeSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new UserFriendlyException("Slug do jogo é obrigatório.");

        Slug = slug.Trim().ToLowerInvariant();
    }
}
```

## Exemplo de Application Service

```csharp
public class GameCatalogAppService : ApplicationService, IGameCatalogAppService
{
    private readonly IRepository<Game, Guid> _gameRepository;
    private readonly IGameCatalogCache _cache;

    public GameCatalogAppService(
        IRepository<Game, Guid> gameRepository,
        IGameCatalogCache cache)
    {
        _gameRepository = gameRepository;
        _cache = cache;
    }

    public async Task<HomeResponseDto> GetHomeAsync()
    {
        var cached = await _cache.GetHomeAsync(CancellationToken);
        if (cached != null) return cached;

        // fetch from DB, build response, cache it
    }

    [AllowAnonymous]
    public async Task<GameDetailDto?> GetBySlugAsync(string slug)
    {
        // implementation
    }
}
```

Application Services são public no `GameHub.Application` e expõem interfaces em `GameHub.Application.Shared` quando exigido pelo template ABP/EAF. Não devem conter regras de domínio; apenas orquestram casos de uso, mapeiam entidades para DTOs e chamam abstrações (`IGameCatalogCache`, `IRepository<T>`).

## Comunicação entre Bounded Contexts

- **Domain Events** para comunicação entre Bounded Contexts. Entidades agregam eventos via `AddLocalEvent` (ABP) e o `EventBus` os despacha na mesma Unit of Work (in-process).
- **Exemplo**: `GamePublishedEvent` →
  - dispara invalidação de cache no contexto Catalog (`InvalidateHomeAsync`, `InvalidateGameDetailAsync`);
  - dispara criação de snapshot no contexto Gameplay Analytics.
- **ABP EventBus** para in-process events. Há `ILocalEventHandler<T>` e `IDistributedEventHandler<T>`. No MVP usamos apenas o local, já que o monólito é single-process.
- **Outbox (futuro)**: para extração futura de microservices, os mesmos eventos podem ser persistidos em uma tabela _Outbox_ e publicados via outbox pattern (e.g., Kafka/RabbitMQ) quando a plataforma for além do MVP.

## Permissions

Plataforma usa o sistema de permissões nativo do ABP/EAF. Ver especificação dedicada: `12-rbac-permissions.md`.

Resumo:

- **AuthorizationProvider**: cada módulo do domínio (Games, Builds, Moderation, Developer, Managers) define um `AuthorizationProvider` que registra as permissões do módulo.
- **Permission names**: hierarquia por módulo (e.g., `GameHub.Games`, `GameHub.Games.Create`, `GameHub.Games.Publish`, `GameHub.Moderation.Review`, `GameHub.Admin.ManageCategories`).
- **Check em runtime**: `[Authorize(Permission = "GameHub.Games.Publish")]` em Application Services ou controllers, e `IsGrantedAsync(...)` em queries condicionais.
- **Roles**: `Admin`, `Moderator`, `Developer`, `Player`, `Guest`. Roles são mapeadas para permissões em `IdentityRole` no seed.
- **Multi-app**: permissões são sempre validadas no backend. O frontend admin usa guards que checam permissões via `PermissionService` (ver `13-frontend-routing.md`).
- **Public endpoints**: `GameCatalogAppService.GetHomeAsync` e similares são marcados `[AllowAnonymous]` quando não exigirem usuário autenticado.

## Soft delete

Entidades que usam `ISoftDelete` do ABP:

- Game
- GameBuild
- DeveloperProfile
- ModerationReview
- UserReport
- Category
- Tag

Entidades que usam hard delete:

- PlaySession
- GameplayEvent
- GameMetricSnapshot
- LeaderboardEntry
