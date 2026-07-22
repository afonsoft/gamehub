# 16 - Plano de Implementação dos Gaps

> Status: Draft
> Gerado a partir da análise comparativa entre `.specs/` e implementação atual do repositório `afonsoft/gamehub`.

---

## Fase 1 — Backend Core: Build Publication + Public URL + Game Shell

**Objetivo:** fazer upload de build gerar uma URL jogável e associá-la ao `Game` quando publicado.

### 1.1 `GameBuildAppService.UploadBuildAsync`

Após `assetStorage.StoreAsync`:
- Extrair o ZIP em memória para localizar `index.html`.
- Calcular `PublicBaseUrl` e `IndexHtmlPath` a partir do endpoint MinIO + bucket + key prefix.
- Persistir em `GameBuild.PublicBaseUrl` e `GameBuild.IndexHtmlPath` (colunas já existentes).

### 1.2 `AdminGameAppService.PublishAsync`

```csharp
public async Task PublishAsync(PublishGameInput input)
{
    var game = await _gameRepository.GetAsync(input.GameId);
    var build = await _buildRepository.GetAsync(input.GameBuildId);

    build.Publish();                // define build.Status = Published
    game.SetPublishedBuild(build);  // associa PublishedBuildId + copia URLs

    await _catalogCache.InvalidateHomeAsync();
    await CurrentUnitOfWork.SaveChangesAsync();
}
```

### 1.3 Métodos de domínio

- `Game.Publish()` define `Status = Published`.
- `Game.SetPublishedBuild(GameBuild build)` seta `PublishedBuildId`, `PublishedBuildUrl` e `PublishedBuild`.

### 1.4 `GameplayAppService.StartSessionAsync`

- Incrementar `TotalPlays` no `Game` quando a sessão inicia.
- Diferenciar requisições duplicadas no mesmo `sessionId` para evitar contagem dupla.

### Arquivos afetados
- `Api/src/GameHub.Application/Builds/GameBuildAppService.cs`
- `Api/src/GameHub.Application/Admin/AdminGameAppService.cs`
- `Api/src/GameHub.Application/Gameplay/GameplayAppService.cs`
- `Api/src/GameHub.Core/Domain/Catalog/Game.cs`
- `Api/src/GameHub.Core/Domain/Builds/GameBuild.cs`

---

## Fase 2 — Cache e Leaderboard em Redis

**Objetivo:** substituir caches in-memory por Redis, conforme especificado.

### 2.1 `RedisGameCatalogCache : IGameCatalogCache`

```csharp
public class RedisGameCatalogCache : IGameCatalogCache
{
    private readonly IDistributedCache _cache;

    public RedisGameCatalogCache(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<HomeResponseDto> GetHomeAsync(CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync("gamehub:catalog:home", cancellationToken);
        return json == null ? null : JsonSerializer.Deserialize<HomeResponseDto>(json);
    }

    public async Task SetHomeAsync(HomeResponseDto dto, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        await _cache.SetStringAsync(
            "gamehub:catalog:home",
            JsonSerializer.Serialize(dto),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
            cancellationToken);
    }

    public async Task InvalidateHomeAsync(CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync("gamehub:catalog:home", cancellationToken);
    }
}
```

### 2.2 `RedisLeaderboardCache : ILeaderboardCache`

```csharp
public class RedisLeaderboardCache : ILeaderboardCache
{
    private readonly IConnectionMultiplexer _redis;

    public RedisLeaderboardCache(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task SubmitScoreAsync(Guid gameId, long userId, long score)
    {
        var db = _redis.GetDatabase();
        await db.SortedSetIncrementAsync($"leaderboard:{gameId:N}", userId.ToString(), score);
    }

    public async Task<List<LeaderboardEntryDto>> GetTopAsync(Guid gameId, int take)
    {
        var db = _redis.GetDatabase();
        var entries = await db.SortedSetRangeByRankWithScoresAsync(
            $"leaderboard:{gameId:N}",
            order: Order.Descending,
            take: take);

        // Converter userId → displayName requer lookup adicional ou cache.
        return entries.Select((e, i) => new LeaderboardEntryDto
        {
            Rank = i + 1,
            UserId = long.Parse(e.Element),
            Score = (long)e.Score
        }).ToList();
    }
}
```

### 2.3 Snapshot no banco

`LeaderboardAppService.SubmitScoreAsync` mantém `LeaderboardEntry` com melhor pontuação por usuário (`GameId + UserId` unique).

### 2.4 Registro DI

```csharp
IocManager.Register<IGameCatalogCache, RedisGameCatalogCache>(DependencyLifeStyle.Transient);
IocManager.Register<ILeaderboardCache, RedisLeaderboardCache>(DependencyLifeStyle.Transient);
```

Adicionar `StackExchange.Redis` em `GameHub.Web.Host.csproj`.

### Arquivos afetados
- `Api/src/GameHub.Application/Catalog/IGameCatalogCache.cs`
- `Api/src/GameHub.Application/Catalog/InMemoryGameCatalogCache.cs` (remover)
- `Api/src/GameHub.Application/Gameplay/ILeaderboardCache.cs`
- `Api/src/GameHub.Application/Gameplay/InMemoryLeaderboardCache.cs` (remover)
- Novos: `Api/src/GameHub.Web.Host/Caching/RedisGameCatalogCache.cs`, `RedisLeaderboardCache.cs`
- `Api/src/GameHub.Application/ProjectNameApplicationModule.cs`
- `Api/src/GameHub.Web.Host/GameHub.Web.Host.csproj`
- `Api/src/GameHub.Web.Host/Startup/WebHostModule.cs`
- `Api/src/GameHub.Web.Host/appsettings*.json`

---

## Fase 3 — Segurança: CSP, Headers, Rate Limit, CORS, JWT HttpOnly

**Objetivo:** alinhar com `15-csp-security-headers.md`.

### 3.1 `SecurityHeadersMiddleware`

```csharp
public async Task Invoke(HttpContext httpContext)
{
    var headers = httpContext.Response.Headers;

    AddHeaderIfNotExists(headers, "X-Content-Type-Options", "nosniff");
    AddHeaderIfNotExists(headers, "X-Frame-Options", "DENY");
    AddHeaderIfNotExists(headers, "X-XSS-Protection", "0");
    AddHeaderIfNotExists(headers, "Referrer-Policy", "strict-origin-when-cross-origin");
    AddHeaderIfNotExists(headers, "Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=()");
    AddHeaderIfNotExists(headers, "Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    AddHeaderIfNotExists(headers, "X-Permitted-Cross-Domain-Policies", "none");
    AddHeaderIfNotExists(headers, "Cross-Origin-Resource-Policy", "same-site");

    headers.Remove("Server");
    headers.Remove("X-Powered-By");

    // Override para game shell
    if (httpContext.Request.Path.StartsWithSegments("/play"))
    {
        headers["X-Frame-Options"] = "SAMEORIGIN";
    }

    await _next(httpContext);
}
```

### 3.2 `ContentSecurityPolicyMiddleware`

```csharp
private static string BuildProductionCsp() =>
    string.Join("; ", new[]
    {
        "default-src 'self'",
        "script-src 'self'",
        "style-src 'self' 'unsafe-inline'",
        "img-src 'self' data: https://gamehub.afonsoft.dev https://gamehub-api.afonsoft.dev",
        "font-src 'self'",
        "connect-src 'self' https://gamehub-api.afonsoft.dev wss://gamehub-api.afonsoft.dev",
        "frame-src https://games.afonsoft.dev",
        "frame-ancestors 'self' https://gamehub.afonsoft.dev",
        "object-src 'none'",
        "base-uri 'self'",
        "form-action 'self'",
        "upgrade-insecure-requests"
    });

private static string BuildDevelopmentCsp() =>
    string.Join("; ", new[]
    {
        "default-src 'self'",
        "script-src 'self' 'unsafe-eval' 'unsafe-inline'",
        "style-src 'self' 'unsafe-inline' 'unsafe-eval'",
        "img-src 'self' data: https://gamehub.afonsoft.dev https://gamehub-api.afonsoft.dev",
        "font-src 'self'",
        "connect-src 'self' https://gamehub-api.afonsoft.dev wss://gamehub-api.afonsoft.dev http://localhost:* ws://localhost:*",
        "frame-src https://games.afonsoft.dev",
        "frame-ancestors 'self' https://gamehub.afonsoft.dev",
        "object-src 'none'",
        "base-uri 'self'",
        "form-action 'self'"
    });
```

- Produção: header `Content-Security-Policy`.
- Desenvolvimento: header `Content-Security-Policy-Report-Only`.

### 3.3 Rate Limiting

Substituir `RateLimitingMiddleware` por `GameHubRateLimiter` usando `System.Threading.RateLimiting` ou contador distribuído Redis.

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("gamehub", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            ip,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});
```

- Headers de resposta: `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`.

### 3.4 CORS

```csharp
public static class CorsConfiguration
{
    public const string HubPolicy = "GameHubCors";
    public const string AdminPolicy = "GameHubAdminCors";

    public static IServiceCollection AddGameHubCors(this IServiceCollection services, IConfiguration configuration)
    {
        var hubOrigins = configuration.GetSection("Cors:HubOrigins").Get<string[]>()
            ?? new[] { "https://gamehub.afonsoft.dev", "http://localhost:4200" };

        var adminOrigins = configuration.GetSection("Cors:AdminOrigins").Get<string[]>()
            ?? new[] { "https://gamehub-admin.afonsoft.dev", "http://localhost:4201" };

        services.AddCors(options =>
        {
            options.AddPolicy(HubPolicy, policy =>
            {
                policy.WithOrigins(hubOrigins)
                    .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                    .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With", "X-Correlation-ID")
                    .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset")
                    .AllowCredentials()
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(600));
            });

            options.AddPolicy(AdminPolicy, policy =>
            {
                policy.WithOrigins(adminOrigins)
                    .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                    .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With", "X-Correlation-ID")
                    .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset")
                    .AllowCredentials()
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(600));
            });
        });

        return services;
    }
}
```

### 3.5 JWT HttpOnly Cookie + Refresh Token

- Endpoint `POST /api/TokenAuth/Authenticate` emite:
  - `GameHub.Auth` (access token) — HttpOnly, Secure, SameSite=Strict, Path=/, MaxAge=2h.
  - `GameHub.Refresh` (refresh token) — HttpOnly, Secure, SameSite=Strict, Path=/api/TokenAuth/Refresh, MaxAge=30d.
- Endpoint `POST /api/TokenAuth/Refresh` valida refresh token e reemite ambos.
- `ITokenRevocationService` com Redis blacklist:
  - `revoked:token:{jti}`
  - `revoked:user:{userId}`

### Arquivos afetados
- `Api/src/GameHub.Web.Host/Middleware/SecurityHeadersMiddleware.cs`
- `Api/src/GameHub.Web.Host/Middleware/ContentSecurityPolicyMiddleware.cs`
- `Api/src/GameHub.Web.Host/Middleware/RateLimitingMiddleware.cs` (remover)
- Novos: `Api/src/GameHub.Web.Host/Configuration/CorsConfiguration.cs`, `Api/src/GameHub.Web.Host/Security/TokenRevocationService.cs`
- `Api/src/GameHub.Web.Host/Startup/Startup.cs`
- `Api/src/GameHub.Web.Host/Startup/WebHostModule.cs`
- `Api/src/GameHub.Web.Host/appsettings*.json`
- Controllers/TokenAuth existentes (verificar path real)

---

## Fase 4 — RBAC e Permission Seeding

**Objetivo:** garantir que permissões do spec existam e sejam checadas.

### 4.1 `GameHubPermissions`

Manter constantes alinhadas com `12-rbac-permissions.md`:
- `Pages.Games.*`, `Pages.Builds.*`, `Pages.Moderation.*`
- `Pages.Categories.*`, `Pages.Tags.*`
- `Pages.Dashboard.*`, `Pages.Users.*`
- `Pages.Gameplay`, `Pages.Leaderboard`

### 4.2 `[AbpAuthorize]` nos AppServices

```csharp
[AbpAuthorize(GameHubPermissions.GamesCreate)]
public async Task<GameDetailDto> CreateDraftAsync(CreateGameDraftInput input) { ... }

[AbpAuthorize(GameHubPermissions.BuildsUpload)]
public async Task<UploadGameBuildResultDto> UploadBuildAsync(...) { ... }

[AbpAuthorize(GameHubPermissions.GamesPublish)]
public async Task PublishAsync(PublishGameInput input) { ... }

[AbpAuthorize(GameHubPermissions.ModerationComplete)]
public async Task<ModerationReviewDto> CompleteReviewAsync(CompleteReviewInput input) { ... }
```

### 4.3 Seed de roles/permissões

Criar `GameHubRoleAndUserSeeder` executado em `SeedHelper.SeedHostDb`:
- SuperAdmin: todas.
- Admin: todas menos moderação avançada.
- Moderator: `Pages.Moderation.*`, `Pages.Builds.Approve`, `Pages.Builds.Reject`.
- Developer: `Pages.Games.*` (own), `Pages.Builds.Upload`, `Pages.Builds.View`, `Pages.Gameplay`, `Pages.Leaderboard`.
- Player: `Pages.Gameplay`, `Pages.Leaderboard`, `Pages.Games.View`.

### Arquivos afetados
- `Api/src/GameHub.Core/Application/Authorization/GameHubPermissions.cs`
- `Api/src/GameHub.Core/Application/Authorization/ProjectNameAuthorizationProvider.cs`
- `Api/src/GameHub.Application/*/*.cs` (adicionar attributes)
- Novo: `Api/src/GameHub.EntityFrameworkCore/Migrations/Seed/Host/GameHubRoleAndUserSeeder.cs`
- `Api/src/GameHub.EntityFrameworkCore/Migrations/Seed/Host/InitialHostDbBuilder.cs` / `SeedHelper.cs`

---

## Fase 5 — Frontend Game Hub (`angular/`)

**Objetivo:** alinhar rotas, guards, resolvers e iframe security.

### 5.1 `app.routes.ts`

```typescript
export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'games', component: CatalogPageComponent },
  { path: 'games/:slug', resolve: { game: gameDetailResolver }, component: GameDetailComponent },
  { path: 'search', component: SearchPageComponent },
  { path: 'play/:slug', canActivate: [authGuard], component: GameFrameComponent },
  { path: 'leaderboard/:gameId', component: LeaderboardComponent },
  { path: 'login', canActivate: [guestGuard], component: LoginComponent },
  { path: 'register', canActivate: [guestGuard], component: RegisterComponent },
  {
    path: 'developer',
    canActivate: [authGuard, permissionGuard(GameHubPermissions.GamesCreate)],
    loadChildren: () => import('./developer/developer.routes').then(m => m.DEVELOPER_ROUTES)
  },
  { path: '**', component: NotFoundComponent }
];
```

### 5.2 Novos arquivos

- `angular/src/app/public/resolvers/game-detail.resolver.ts`
- `angular/src/app/developer/resolvers/build-list.resolver.ts`
- `angular/src/app/core/guards/permission.guard.ts`
- `angular/src/app/core/constants/permissions.ts`
- `angular/src/app/public/pages/search-page/search-page.component.ts`
- `angular/src/app/public/pages/not-found/not-found.component.ts`
- `angular/src/app/public/pages/catalog-page/catalog-page.component.ts`

### 5.3 `GameFrameComponent`

```typescript
private postToGame(message: unknown): void {
  const contentWindow = this.frame?.nativeElement?.contentWindow;
  contentWindow?.postMessage(message, environment.gameOrigin);
}
```

`environment.gameOrigin` = `https://games.afonsoft.dev`.

### 5.4 AuthService e TokenService

- Remover `localStorage` do `TokenService`.
- `AuthService` passa a confiar no cookie HttpOnly enviado automaticamente.
- Implementar refresh automático quando 401 for retornado.

### Arquivos afetados
- `angular/src/app/app.routes.ts`
- `angular/src/app/core/auth/auth.guard.ts`
- `angular/src/app/core/auth/guest.guard.ts`
- `angular/src/app/core/auth/developer.guard.ts`
- `angular/src/app/core/auth/auth.service.ts`
- `angular/src/app/core/auth/token.service.ts`
- `angular/src/app/player/game-frame/game-frame.component.ts`
- Novos componentes/resolvers/guards/constants

---

## Fase 6 — Frontend Admin (`angular-admin/GameHub.UI`)

**Objetivo:** substituir módulo legacy por rotas do spec.

### 6.1 Novo `admin.routes.ts`

```typescript
export const ADMIN_ROUTES: Routes = [
  { path: 'login', canActivate: [guestGuard], component: LoginPageComponent },
  {
    path: '',
    canActivate: [authGuard, adminGuard],
    children: [
      { path: '', redirectTo: 'games', pathMatch: 'full' },
      { path: 'games', component: GameListComponent },
      { path: 'games/:id', component: GameDetailComponent, resolve: { game: gameDetailResolver } },
      { path: 'games/:id/edit', component: GameEditComponent, resolve: { game: gameDetailResolver } },
      { path: 'moderation', component: ReviewQueueComponent, canActivate: [moderatorGuard] },
      { path: 'moderation/:id', component: ReviewDetailComponent, canActivate: [moderatorGuard], resolve: { review: moderationDetailResolver } },
      { path: 'categories', component: CategoryListComponent },
      { path: 'categories/create', component: CategoryEditComponent },
      { path: 'categories/:id/edit', component: CategoryEditComponent, resolve: { category: categoryEditResolver } },
      { path: 'tags', component: TagListComponent },
      { path: 'tags/create', component: TagEditComponent },
      { path: 'tags/:id/edit', component: TagEditComponent, resolve: { tag: tagEditResolver } },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'dashboard/flags', component: FeatureFlagsComponent },
      { path: 'dashboard/audit', component: AuditLogComponent },
      { path: '**', component: NotFoundComponent }
    ]
  }
];
```

### 6.2 Novos arquivos

- `angular-admin/GameHub.UI/src/app/core/guards/auth.guard.ts`
- `angular-admin/GameHub.UI/src/app/core/guards/admin.guard.ts`
- `angular-admin/GameHub.UI/src/app/core/guards/moderator.guard.ts`
- `angular-admin/GameHub.UI/src/app/core/guards/guest.guard.ts`
- `angular-admin/GameHub.UI/src/app/resolvers/game-detail.resolver.ts`
- `angular-admin/GameHub.UI/src/app/resolvers/moderation-detail.resolver.ts`
- `angular-admin/GameHub.UI/src/app/resolvers/category-edit.resolver.ts`
- `angular-admin/GameHub.UI/src/app/resolvers/tag-edit.resolver.ts`
- Novos componentes em `pages/` para `GameList`, `GameDetail`, `GameEdit`, `ReviewQueue`, `ReviewDetail`, `CategoryEdit`, `TagEdit`.

### Arquivos afetados
- `angular-admin/GameHub.UI/src/app/app-routing.module.ts`
- `angular-admin/GameHub.UI/src/app/main/main-routing.module.ts`
- `angular-admin/GameHub.UI/src/app/main/gamehub/gamehub-routing.module.ts`

---

## Fase 7 — DevOps e Infra

**Objetivo:** completar Docker Compose e configuração para produção.

### 7.1 Criar `docker-compose.yml` na raiz

Unir `docker-compose.app.yml` + `docker-compose.infra.yml`:
- `backend`, `angular-hub`, `angular-admin`.
- `postgres`, `redis`, `minio`.
- Rede `gamehub`, volumes `pgdata`, `minio-data`.

### 7.2 Configuração padrão

- `appsettings*.json`: `Database.Provider: PostgreSQL`.
- `OpenTelemetry:OtlpEndpoint`: ler de `OTEL_EXPORTER_OTLP_ENDPOINT`.
- `Storage__Minio__Endpoint`: `http://host.docker.internal:9000` em dev, DNS real em produção.

### 7.3 `install.sh`

- Suportar flag `--infra` para subir infra junto.
- Ou documentar uso combinado: `docker compose -f docker-compose.yml up -d`.

### Arquivos afetados
- `docker-compose.yml` (novo)
- `docker-compose.app.yml`
- `docker-compose.infra.yml`
- `Api/src/GameHub.Web.Host/appsettings*.json`
- `.env.example`
- `install.sh`

---

## Fase 8 — Observabilidade, Jobs e LGPD

**Objetivo:** métricas agregadas e compliance.

### 8.1 Hangfire Job

```csharp
public class GameMetricsAggregationJob : BackgroundJob<GameMetricsAggregationArgs>, ITransientDependency
{
    private readonly IRepository<GameMetricSnapshot, Guid> _snapshotRepository;
    private readonly IRepository<PlaySession, Guid> _sessionRepository;
    private readonly IRepository<GameplayEvent, Guid> _eventRepository;

    public override void Execute(GameMetricsAggregationArgs args)
    {
        var date = args.Date;
        // agrega por gameId: plays, uniquePlayers, avgDuration, loadingFinishedCount, errorCount, commercialBreakCount, rewardedBreakCount
    }
}
```

Agendamento em `Startup`:
```csharp
RecurringJob.AddOrUpdate<GameMetricsAggregationJob>("metrics-aggregation", j => j.Execute(new GameMetricsAggregationArgs { Date = DateTime.UtcNow.Date.AddDays(-1) }), Cron.Daily);
```

### 8.2 LGPD

```csharp
public interface IPrivacyAppService
{
    Task<UserDataExportDto> ExportUserDataAsync(long userId);
    Task DeleteUserDataAsync(long userId);
}
```

- Exportar dados pessoais (PlaySession, GameplayEvent, LeaderboardEntry, DeveloperProfile).
- Anonimizar/deletar mediante soft-delete.

### Arquivos afetados
- Novo: `Api/src/GameHub.Core/Domain/Jobs/GameMetricsAggregationJob.cs`
- `Api/src/GameHub.Web.Host/Startup/Startup.cs`
- Novo: `Api/src/GameHub.Application/Privacy/PrivacyAppService.cs`
- `Api/src/GameHub.Application/Admin/AdminDashboardAppService.cs`

---

## Fase 9 — Remoção do Domínio `Airplanes`

**Objetivo:** limpar template legado.

### 9.1 Backend
- Remover `Api/src/GameHub.Core/Airplanes/`.
- Remover `Api/src/GameHub.Application/Airplanes/`.
- Remover DbSet `Airplanes` de `ProjectNameDbContext`.
- Remover permissões `Pages.Airplanes`.
- Gerar migration `RemoveAirplanes`.

### 9.2 Frontend Admin
- Remover `angular-admin/GameHub.UI/src/app/main/airplanes/`.
- Remover rota `/airplanes` de `main-routing.module.ts`.

### Arquivos afetados
- `Api/src/GameHub.Core/Airplanes/` (remover)
- `Api/src/GameHub.Application/Airplanes/` (remover)
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/ProjectNameDbContext.cs`
- `Api/src/GameHub.Core/Application/Authorization/ProjectNameAuthorizationProvider.cs`
- `Api/src/GameHub.Core/Application/Authorization/ProjectNamePermissions.cs`
- `angular-admin/GameHub.UI/src/app/main/main-routing.module.ts`
- `angular-admin/GameHub.UI/src/app/main/airplanes/` (remover)

---

## Fase 10 — Testes e Documentação

**Objetivo:** cobertura e rastreabilidade.

### 10.1 Testes
- `GameBuildPackageValidator` (index.html, extensões bloqueadas, tamanho, sha256).
- `GameBuildAppService` (upload, extração, publicação).
- `AdminGameAppService` (publish, suspend, approve build).
- `RedisGameCatalogCache` (get/set/invalidate).
- `RedisLeaderboardCache` (submit, top ranking).
- `ContentSecurityPolicyMiddleware` e `SecurityHeadersMiddleware` (headers corretos).
- `CorsConfiguration` (preflight, credenciais).
- `TokenRevocationService` (revoke, check, user revoke).

### 10.2 Documentação
- Atualizar `docs/agent-execution-log.md`.
- Atualizar `README.md` se houver mudanças de configuração.
- Manter `CHANGELOG.md`.

---

## Ordem sugerida de execução

1. **Fase 9** — remover Airplanes (simplifica base).
2. **Fase 1** — build publication (desbloqueia game shell).
3. **Fase 2** — Redis cache/leaderboard (performance).
4. **Fase 4** — RBAC (autorização).
5. **Fase 3** — segurança (protege API).
6. **Fase 5 e 6** — frontends (entrega UX).
7. **Fase 7 e 8** — infra/observabilidade/LGPD.
8. **Fase 10** — testes/docs.
