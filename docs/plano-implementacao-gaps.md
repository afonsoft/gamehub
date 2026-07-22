# Plano de Implementação — Gaps (`.specs/16-plano-implementacao-gaps.md`)

> **Status:** Draft — aguardando aprovação para iniciar.  
> **Baseline:** `main` atualizada via `git pull` e build/testes passando após `dotnet restore`.
>
> Comandos de baseline verificados:
> - `dotnet restore Api/GameHub.sln`
> - `dotnet build Api/GameHub.sln -c Release --no-restore`
> - `dotnet test Api/GameHub.sln -c Release --no-build` → **228 passed / 1 skipped**

---

## 1. Escopo proposto

Implementar o plano em **4 Pull Requests** independentes, seguindo a ordem sugerida pelo `.specs/16`:

1. **PR-1 — Limpeza e base do domínio** (Fase 9 + Fase 1)
2. **PR-2 — Cache, leaderboard e RBAC** (Fase 2 + Fase 4)
3. **PR-3 — Segurança da API** (Fase 3 — headers/CSP/CORS/rate-limit; JWT HttpOnly em PR separado)
4. **PR-4 — Frontends, observabilidade, DevOps e LGPD** (Fases 5, 6, 7, 8 e testes)

A **JWT HttpOnly/Refresh Token** (Fase 3.5) pode ser implementada, mas exige alteração no fluxo de autenticação do `Eaf.Middleware.Web.Core` 9.2.0. Por isso será entregue em **PR dedicado**, com duas abordagens descritas abaixo.

---

## 2. PR-1 — Limpeza do template e publicação de builds

### 2.1 Fase 9: Remover Airplanes

- Deletar pastas:
  - `Api/src/GameHub.Core/Airplanes/`
  - `Api/src/GameHub.Application/Airplanes/`
  - `Api/test/GameHub.Tests/Airplanes/`
  - `angular-admin/GameHub.UI/src/app/main/airplanes/`
- `Api/src/GameHub.EntityFrameworkCore/EntityFrameworkCore/GameHubDbContext.cs`
  - Remover: `public virtual DbSet<Airplane> Airplanes { get; set; }`
  - Remover: `using GameHub.Airplanes;`
- `Api/src/GameHub.Core/Application/Authorization/GameHubPermissions.cs`
  - Remover constantes `Pages_Airplanes_*`.
- `Api/src/GameHub.Core/Application/Authorization/GameHubAuthorizationProvider.cs`
  - Remover criação das permissões `Airplanes`.
- `angular-admin/GameHub.UI/src/app/main/main-routing.module.ts`
  - Remover a rota `/airplanes`.
- `Api/src/GameHub.EntityFrameworkCore/Migrations/`
  - Gerar `dotnet ef migrations add RemoveAirplanes`.

### 2.2 Fase 1: Build Publication

#### 2.2.1 Extração do ZIP e cálculo de URLs públicas

**Alterar `Api/src/GameHub.Application/Developer/Dto/ValidationSummaryDto.cs`:**

```csharp
public class ValidationSummaryDto
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public long SizeBytes { get; set; }
    public string HashSha256 { get; set; } = string.Empty;
    public bool HasIndexHtml { get; set; }

    /// <summary>Caminho relativo do index.html dentro do ZIP.</summary>
    public string IndexHtmlPath { get; set; } = string.Empty;
}
```

**Alterar `Api/src/GameHub.Application/Builds/GameBuildPackageValidator.cs`:**

```csharp
public async Task<ValidationSummaryDto> ValidateAsync(Stream packageStream, CancellationToken cancellationToken = default)
{
    // ... validações existentes de tamanho, sha256 e extensões bloqueadas ...

    var indexEntry = entries.FirstOrDefault(e =>
        e.FullName.Equals("index.html", StringComparison.OrdinalIgnoreCase));

    summary.HasIndexHtml = indexEntry != null;
    summary.IndexHtmlPath = summary.HasIndexHtml ? indexEntry.FullName : string.Empty;

    summary.IsValid = !summary.Errors.Any();
    return summary;
}
```

**Alterar `Api/src/GameHub.Core/Storage/StoredAsset.cs`:**

```csharp
public class StoredAsset
{
    /// <summary>URL do pacote original (ZIP).</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Prefixo público dos arquivos extraídos do build.</summary>
    public string PublicBaseUrl { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;
    public string ETag { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}
```

**Alterar `Api/src/GameHub.Web.Host/Storage/MinioGameAssetStorage.cs`:**

```csharp
public async Task<StoredAsset> StoreAsync(GameBuildPackage package, CancellationToken cancellationToken = default)
{
    if (package?.Content == null)
        throw new ArgumentNullException(nameof(package));

    var prefix = $"builds/{package.GameId:N}/{package.BuildId:N}/";
    var packageKey = $"{prefix}{package.FileName}";

    await EnsureBucketExistsAsync(cancellationToken);

    // 1. Faz upload de cada arquivo extraído do ZIP para o prefixo
    string indexHtmlPath = string.Empty;
    using (var zip = new ZipArchive(package.Content, ZipArchiveMode.Read, leaveOpen: true))
    {
        foreach (var entry in zip.Entries.Where(e => !string.IsNullOrWhiteSpace(e.Name)))
        {
            await UploadEntryAsync(entry, prefix, cancellationToken);
            if (entry.FullName.Equals("index.html", StringComparison.OrdinalIgnoreCase))
                indexHtmlPath = entry.FullName;
        }
    }

    // 2. Faz upload do próprio ZIP para auditoria/reprocessamento
    package.Content.Position = 0;
    var packageRequest = new PutObjectRequest
    {
        BucketName = _options.Minio.Bucket,
        Key = packageKey,
        InputStream = package.Content,
        ContentType = package.ContentType ?? "application/octet-stream",
        AutoCloseStream = false
    };
    var packageResponse = await _s3Client.PutObjectAsync(packageRequest, cancellationToken);

    return new StoredAsset
    {
        Key = packageKey,
        ETag = packageResponse.ETag,
        SizeBytes = package.Content.Length,
        Url = BuildPublicUrl(packageKey),
        PublicBaseUrl = BuildPublicUrl(prefix),
    };
}

// novo método auxiliar
private async Task UploadEntryAsync(ZipArchiveEntry entry, string prefix, CancellationToken cancellationToken)
{
    // define content-type por extensão; faz PutObjectRequest
}
```

**Nota:** `MinioGameAssetStorage` atualmente guarda apenas o ZIP. O plano passa a **descompactar e publicar os arquivos** em `builds/{gameId}/{buildId}/`, mantendo o ZIP original para auditoria.

#### 2.2.2 Persistência no GameBuild

**Alterar `Api/src/GameHub.Application/Builds/GameBuildAppService.cs`:**

```csharp
public async Task<UploadGameBuildResultDto> UploadBuildAsync(Guid gameId, Stream packageStream, string fileName, string contentType)
{
    // ... validações de tamanho e chamada ao _validator ...

    var buildId = Guid.NewGuid();
    var buildNumber = (await _buildRepository.CountAsync(b => b.GameId == gameId)) + 1;

    packageStream.Position = 0;
    var asset = await _assetStorage.StoreAsync(new GameBuildPackage
    {
        GameId = gameId,
        BuildId = buildId,
        FileName = fileName,
        ContentType = contentType,
        Content = packageStream
    });

    var build = new GameBuild(
        buildId,
        gameId,
        fileName,
        buildNumber,
        asset.Url,
        validation.SizeBytes,
        validation.HashSha256)
    {
        PublicBaseUrl = asset.PublicBaseUrl,
        IndexHtmlPath = validation.IndexHtmlPath,
        Status = GameBuildStatus.Validated
    };

    await _buildRepository.InsertAsync(build);
    await CurrentUnitOfWork.SaveChangesAsync();

    return new UploadGameBuildResultDto
    {
        BuildId = buildId,
        Version = fileName,
        Status = build.Status.ToString(),
        ValidationSummary = string.Join("; ", validation.Errors)
    };
}
```

**Alterar `Api/src/GameHub.Core/Domain/Catalog/Game.cs`:**

```csharp
public void Publish(Guid? buildId = null)
{
    if (Status != GameStatus.InReview && Status != GameStatus.Draft)
        throw new InvalidOperationException($"Cannot publish game with status {Status}.");

    var build = buildId.HasValue
        ? FindBuild(buildId.Value)
        : GameBuilds.FirstOrDefault(b => b.Status == GameBuildStatus.Approved);

    if (build == null)
        throw new InvalidOperationException("No approved build found for this game.");

    if (build.Status != GameBuildStatus.Approved && build.Status != GameBuildStatus.Published)
        throw new InvalidOperationException("Build must be approved before publishing.");

    PublishedBuildId = build.Id;
    Status = GameStatus.Published;
}

/// <summary>Associa o build publicado ao jogo.</summary>
public void SetPublishedBuild(GameBuild build)
{
    if (build == null) throw new ArgumentNullException(nameof(build));
    if (build.GameId != Id) throw new InvalidOperationException("Build does not belong to this game.");
    Publish(build.Id);
}
```

**Alterar `Api/src/GameHub.Application/Admin/AdminGameAppService.cs`:**

```csharp
public async Task PublishAsync(PublishGameInput input)
{
    var game = await _gameRepository.GetAsync(input.GameId);
    var build = await _buildRepository.GetAsync(input.GameBuildId);

    build.Publish();                // GameBuildStatus -> Published
    game.SetPublishedBuild(build);  // associa PublishedBuildId

    await _catalogCache.InvalidateHomeAsync();
    await CurrentUnitOfWork.SaveChangesAsync();
}
```

#### 2.2.3 Sessão de gameplay e contagem de plays

**Alterar `Api/src/GameHub.Application/Gameplay/Dto/StartPlaySessionInput.cs`:**

```csharp
public class StartPlaySessionInput
{
    [Required] public Guid GameId { get; set; }

    [Required]
    [StringLength(20)]
    public string DeviceType { get; set; } = string.Empty;

    [StringLength(500)]
    public string Browser { get; set; }

    [StringLength(500)]
    public string Referrer { get; set; }

    /// <summary>Idempotency key gerado pelo cliente para evitar plays duplicados em retry.</summary>
    [StringLength(64)]
    public string ClientRequestId { get; set; }
}
```

**Alterar `Api/src/GameHub.Core/Domain/Gameplay/PlaySession.cs`:**

```csharp
public class PlaySession : Entity<Guid>, IMayHaveTenant
{
    // ... propriedades existentes ...

    /// <summary>Idempotency key enviada pelo cliente.</summary>
    [StringLength(64)]
    public string ClientRequestId { get; set; }
}
```

**Alterar `Api/src/GameHub.Application/Gameplay/GameplayAppService.cs`:**

```csharp
public async Task<PlaySessionDto> StartSessionAsync(StartPlaySessionInput input)
{
    var game = await _gameRepository.GetAsync(input.GameId);

    // Idempotência por ClientRequestId
    if (!string.IsNullOrEmpty(input.ClientRequestId))
    {
        var existing = await _playSessionRepository.FirstOrDefaultAsync(
            s => s.GameId == input.GameId && s.ClientRequestId == input.ClientRequestId);

        if (existing != null)
            return ObjectMapper.Map<PlaySessionDto>(existing);
    }

    var session = new PlaySession
    {
        Id = Guid.NewGuid(),
        GameId = input.GameId,
        UserId = AbpSession.UserId,
        StartedAt = DateTime.UtcNow,
        DeviceType = input.DeviceType,
        Browser = input.Browser ?? "Unknown",
        Referrer = input.Referrer,
        ClientRequestId = input.ClientRequestId
    };

    await _playSessionRepository.InsertAsync(session);
    game.TotalPlays++;              // incrementa apenas quando cria nova sessão
    await CurrentUnitOfWork.SaveChangesAsync();

    return ObjectMapper.Map<PlaySessionDto>(session);
}
```

**Adicionar índice em `GameHubDbContext.OnModelCreating` (ou `ConfigureGameHub`):**

```csharp
modelBuilder.Entity<PlaySession>(b =>
{
    b.HasIndex(e => new { e.GameId, e.ClientRequestId }).IsUnique(false);
});
```

**Alterar `angular/src/app/core/services/gameplay-bridge.service.ts`:**

```typescript
startSession(input: StartPlaySessionInput): Observable<PlaySession> {
  const clientRequestId = this.generateClientRequestId(input.gameId);
  return this.http
    .post<PlaySession | { result?: PlaySession }>(
      `${this.gameplayUrl}/StartSession`,
      { ...input, clientRequestId }
    )
    .pipe(map(response => this.unwrap<PlaySession>(response)));
}

private generateClientRequestId(gameId: string): string {
  return `${gameId}-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
}
```

---

## 3. PR-2 — Cache Redis, Leaderboard Redis e RBAC

### 3.1 Fase 2 — Redis Cache & Leaderboard

**Criar `Api/src/GameHub.Web.Host/Caching/RedisGameCatalogCache.cs`:**

```csharp
public class RedisGameCatalogCache : IGameCatalogCache, ITransientDependency
{
    private readonly IDistributedCache _cache;
    private const string HomeKey = "gamehub:catalog:home";

    public RedisGameCatalogCache(IDistributedCache cache) => _cache = cache;

    public async Task<HomeResponseDto> GetHomeAsync(CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync(HomeKey, cancellationToken);
        return json == null ? null : JsonSerializer.Deserialize<HomeResponseDto>(json);
    }

    public async Task SetHomeAsync(HomeResponseDto dto, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        await _cache.SetStringAsync(
            HomeKey,
            JsonSerializer.Serialize(dto),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
            cancellationToken);
    }

    public async Task InvalidateHomeAsync(CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(HomeKey, cancellationToken);
    }
}
```

**Criar `Api/src/GameHub.Web.Host/Caching/RedisLeaderboardCache.cs`:**

```csharp
public class RedisLeaderboardCache : ILeaderboardCache, ITransientDependency
{
    private readonly IConnectionMultiplexer _redis;

    public RedisLeaderboardCache(IConnectionMultiplexer redis) => _redis = redis;

    public async Task SubmitScoreAsync(Guid gameId, long userId, long score, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.SortedSetIncrementAsync($"leaderboard:{gameId:N}", userId.ToString(), score);
    }

    public async Task<IReadOnlyList<LeaderboardEntryDto>> GetTopAsync(Guid gameId, int take, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var entries = await db.SortedSetRangeByRankWithScoresAsync(
            $"leaderboard:{gameId:N}",
            order: Order.Descending,
            take: take);

        return entries.Select((e, i) => new LeaderboardEntryDto
        {
            Rank = i + 1,
            UserId = long.Parse(e.Element!),
            Score = (long)e.Score,
            UpdatedAt = DateTime.UtcNow
        }).ToList();
    }
}
```

**Alterar `Api/src/GameHub.Web.Host/Startup/WebHostModule.cs`:**

```csharp
public override void PreInitialize()
{
    // ... configurações existentes ...

    var redisConn = _appConfiguration["RedisCache:ConnectionString"]
                   ?? _appConfiguration["RedisCache__ConnectionString"];

    if (!string.IsNullOrWhiteSpace(redisConn))
    {
        IocManager.IocContainer.Register(
            Component.For<IConnectionMultiplexer>()
                .Instance(ConnectionMultiplexer.Connect(redisConn))
                .LifestyleSingleton);
    }

    Configuration.ReplaceService<IGameCatalogCache, RedisGameCatalogCache>(DependencyLifeStyle.Transient);
    Configuration.ReplaceService<ILeaderboardCache, RedisLeaderboardCache>(DependencyLifeStyle.Transient);

    // ... resto ...
}
```

**Adicionar pacote em `Api/src/GameHub.Web.Host/GameHub.Web.Host.csproj`:**

```xml
<PackageReference Include="StackExchange.Redis" Version="2.8.31" />
```

> A versão deve ser confirmada com .NET 10 / EAF 9.2.0 antes do merge.

**Remover (ou manter como fallback local):**
- `Api/src/GameHub.Application/Catalog/InMemoryGameCatalogCache.cs`
- `Api/src/GameHub.Application/Gameplay/InMemoryLeaderboardCache.cs`

> **Recomendação:** manter os caches in-memory e registrá-los como fallback quando `RedisCache:IsEnabled` estiver `false`.

### 3.2 Fase 4 — RBAC e Permission Seeding

**Alterar `Api/src/GameHub.Core/Application/Authorization/GameHubPermissions.cs`:**

```csharp
// Games
public const string Pages_Games = "Pages.Games";
public const string Pages_Games_View = "Pages.Games.View";
public const string Pages_Games_Create = "Pages.Games.Create";
public const string Pages_Games_Edit = "Pages.Games.Edit";
public const string Pages_Games_Delete = "Pages.Games.Delete";
public const string Pages_Games_Publish = "Pages.Games.Publish";
public const string Pages_Games_Suspend = "Pages.Games.Suspend";

// Builds
public const string Pages_Builds = "Pages.Builds";
public const string Pages_Builds_Upload = "Pages.Builds.Upload";
public const string Pages_Builds_View = "Pages.Builds.View";
public const string Pages_Builds_Approve = "Pages.Builds.Approve";
public const string Pages_Builds_Reject = "Pages.Builds.Reject";

// Moderation
public const string Pages_Moderation = "Pages.Moderation";
public const string Pages_Moderation_View = "Pages.Moderation.View";
public const string Pages_Moderation_Review = "Pages.Moderation.Review";
public const string Pages_Moderation_Complete = "Pages.Moderation.Complete";

// Categories
public const string Pages_Categories = "Pages.Categories";
public const string Pages_Categories_Manage = "Pages.Categories.Manage";

// Tags
public const string Pages_Tags = "Pages.Tags";
public const string Pages_Tags_Manage = "Pages.Tags.Manage";

// Dashboard
public const string Pages_Dashboard = "Pages.Dashboard";
public const string Pages_Dashboard_View = "Pages.Dashboard.View";
public const string Pages_Dashboard_FeatureFlags = "Pages.Dashboard.FeatureFlags";
public const string Pages_Dashboard_AuditLog = "Pages.Dashboard.AuditLog";

// Reports
public const string Pages_Reports = "Pages.Reports";
public const string Pages_Reports_View = "Pages.Reports.View";
public const string Pages_Reports_Manage = "Pages.Reports.Manage";

// Developer
public const string Pages_Developer = "Pages.Developer";
public const string Pages_Developer_Profile = "Pages.Developer.Profile";
public const string Pages_Developer_Games = "Pages.Developer.Games";

// Users
public const string Pages_Users = "Pages.Users";
public const string Pages_Users_Manage = "Pages.Users.Manage";

// Gameplay
public const string Pages_Gameplay = "Pages.Gameplay";
public const string Pages_Leaderboard = "Pages.Leaderboard";
```

> **Correção:** `Pages_Dashboard` estava como `"Pages.GameHubDashboard"` no código atual, o que diverge do `main-routing.module.ts` (`permission: 'Pages.Dashboard'`). O plano alinha para `"Pages.Dashboard"`.

**Alterar `Api/src/GameHub.Core/Application/Authorization/GameHubAuthorizationProvider.cs`:**

```csharp
public override void SetPermissions(IPermissionDefinitionContext context)
{
    var pages = context.GetPermissionOrNull(MiddlewarePermissions.Pages)
               ?? context.CreatePermission(MiddlewarePermissions.Pages, LEaf("Pages"));

    // Games, Builds, Moderation, Categories, Tags, Dashboard, Reports, Developer, Users, Gameplay, Leaderboard
    RegisterGameHubPermissions(pages);
}
```

**Adicionar `[AbpAuthorize]` nos AppServices:**

- `Api/src/GameHub.Application/Builds/GameBuildAppService.cs`
  - `[AbpAuthorize(GameHubPermissions.Pages_Builds_Upload)]` em `UploadBuildAsync`.
- `Api/src/GameHub.Application/Admin/AdminGameAppService.cs`
  - `[AbpAuthorize(GameHubPermissions.Pages_Builds_Approve)]` em `ApproveBuildAsync`
  - `[AbpAuthorize(GameHubPermissions.Pages_Builds_Reject)]` em `RejectBuildAsync`
  - `[AbpAuthorize(GameHubPermissions.Pages_Games_Publish)]` em `PublishAsync`
  - `[AbpAuthorize(GameHubPermissions.Pages_Games_Suspend)]` em `SuspendAsync`
  - `[AbpAuthorize(GameHubPermissions.Pages_Games)]` em `GetAllAsync` / `GetDetailAsync`
- `Api/src/GameHub.Application/Gameplay/LeaderboardAppService.cs`
  - `[AbpAuthorize(GameHubPermissions.Pages_Leaderboard)]` em `GetTopAsync`
  - `[AbpAuthorize(GameHubPermissions.Pages_Gameplay)]` em `SubmitScoreAsync`
- `Api/src/GameHub.Application/Gameplay/GameplayAppService.cs`
  - `[AbpAuthorize(GameHubPermissions.Pages_Gameplay)]` em `StartSessionAsync` / `StopSessionAsync` / `EventAsync`

> **Nota:** regras "own" (Developer editar apenas seus jogos) precisam de `IAuthorizationService` + query filter e serão tratadas em PR separado, se aprovado.

**Criar `Api/src/GameHub.EntityFrameworkCore/Migrations/Seed/Host/GameHubRoleAndUserSeeder.cs`:**

```csharp
public class GameHubRoleAndUserSeeder
{
    private readonly GameHubDbContext _context;
    private readonly RoleManager _roleManager;
    private readonly UserManager _userManager;
    private readonly IPermissionManager _permissionManager;

    public GameHubRoleAndUserSeeder(
        GameHubDbContext context,
        RoleManager roleManager,
        UserManager userManager,
        IPermissionManager permissionManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _permissionManager = permissionManager;
    }

    public async Task CreateAsync()
    {
        // SuperAdmin, Admin, Moderator, Developer, Player
        // Atribuir permissões conforme matriz de 12-rbac-permissions.md
    }
}
```

**Alterar `Api/src/GameHub.EntityFrameworkCore/Migrations/Seed/SeedHelper.cs` para executar o seeder.**

---

## 4. PR-3 — Segurança (headers, CSP, CORS, rate limit)

### 4.1 Fase 3.1/3.2 — Security Headers e CSP

**Reescrever `Api/src/GameHub.Web.Host/Middleware/SecurityHeadersMiddleware.cs`:**

```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

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

        if (httpContext.Request.Path.StartsWithSegments("/play"))
            headers["X-Frame-Options"] = "SAMEORIGIN";

        headers.Remove("Server");
        headers.Remove("X-Powered-By");

        await _next(httpContext);
    }

    private static void AddHeaderIfNotExists(IHeaderDictionary headers, string key, string value)
    {
        if (!headers.ContainsKey(key)) headers.Append(key, value);
    }
}
```

**Reescrever `Api/src/GameHub.Web.Host/Middleware/ContentSecurityPolicyMiddleware.cs`:**

```csharp
public class ContentSecurityPolicyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public ContentSecurityPolicyMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task Invoke(HttpContext context)
    {
        var header = _env.IsDevelopment()
            ? "Content-Security-Policy-Report-Only"
            : "Content-Security-Policy";

        var value = _env.IsDevelopment() ? BuildDevelopmentCsp() : BuildProductionCsp();

        if (!context.Response.Headers.ContainsKey(header))
            context.Response.Headers.Append(header, value);

        await _next(context);
    }

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
}
```

### 4.2 Fase 3.3 — Rate Limiting

**Criar `Api/src/GameHub.Web.Host/Configuration/RateLimitingConfiguration.cs`:**

```csharp
public static class RateLimitingConfiguration
{
    public static IServiceCollection AddGameHubRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

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

            options.OnRejected = (ctx, _) =>
            {
                ctx.HttpContext.Response.Headers.Append("X-RateLimit-Limit", "100");
                ctx.HttpContext.Response.Headers.Append("X-RateLimit-Remaining", "0");
                ctx.HttpContext.Response.Headers.Append("X-RateLimit-Reset", DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds().ToString());
                return ValueTask.CompletedTask;
            };
        });

        return services;
    }
}
```

**Alterar `Api/src/GameHub.Web.Host/Startup/Startup.cs`:**

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // ...
    services.AddGameHubRateLimiter();
    // ...
}

public void Configure(IApplicationBuilder app, ...)
{
    // ...
    app.UseRateLimiter();
    // ...
}
```

**Remover `Api/src/GameHub.Web.Host/Middleware/RateLimitingMiddleware.cs`.**

### 4.3 Fase 3.4 — CORS

**Criar `Api/src/GameHub.Web.Host/Configuration/CorsConfiguration.cs`:**

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
            options.AddPolicy(HubPolicy, policy => BuildPolicy(policy, hubOrigins));
            options.AddPolicy(AdminPolicy, policy => BuildPolicy(policy, adminOrigins));
        });

        return services;
    }

    private static void BuildPolicy(CorsPolicyBuilder policy, string[] origins)
    {
        policy.WithOrigins(origins)
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With", "X-Correlation-ID")
            .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromSeconds(600));
    }
}
```

**Alterar `Api/src/GameHub.Web.Host/Startup/Startup.cs`:**

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // substituir o bloco services.AddCors(...) por:
    services.AddGameHubCors(_appConfiguration);
}

public void Configure(IApplicationBuilder app, ...)
{
    // seleciona a política por origem usando middleware customizado ou aplica padrão:
    app.UseCors(CorsConfiguration.HubPolicy);
    // ...
}
```

> **Decisão:** para endpoints admin, usar `[EnableCors(CorsConfiguration.AdminPolicy)]` em controllers específicos ou implementar um middleware de seleção dinâmica por `Origin`.

### 4.4 Fase 3.5 — JWT HttpOnly + Refresh Token (PR separado)

> **Risco:** o endpoint `/api/TokenAuth/Authenticate` é fornecido pelo pacote `Eaf.Middleware.Web.Core` 9.2.0. Duas abordagens:

**Opção A — Middleware de interceptação (recomendada):**

1. Criar `Api/src/GameHub.Web.Host/Security/JwtCookieMiddleware.cs`:
   - Intercepta respostas de `/api/TokenAuth/Authenticate`.
   - Lê `accessToken` do body JSON.
   - Escreve cookie `GameHub.Auth` (HttpOnly, Secure, SameSite=Strict, Path=/, MaxAge=2h).
   - Gera refresh token e escreve cookie `GameHub.Refresh` (Path=/api/TokenAuth/Refresh, MaxAge=30d).
   - Remove `accessToken`/`encryptedAccessToken` do body ou documenta que o frontend deve ignorá-los.
2. Criar `Api/src/GameHub.Web.Host/Controllers/TokenAuthController.cs` com `[Route("api/TokenAuth")]` e `POST Refresh` para reemitir tokens.
3. Criar `Api/src/GameHub.Web.Host/Security/TokenRevocationService.cs`:
   ```csharp
   public interface ITokenRevocationService
   {
       Task RevokeTokenAsync(string jti);
       Task<bool> IsTokenRevokedAsync(string jti);
       Task RevokeAllUserTokensAsync(long userId);
   }
   ```
4. Adicionar middleware que lê cookie `GameHub.Auth` e injeta no header `Authorization` antes de `UseJwtTokenMiddleware`.
5. Adicionar middleware pós-autenticação que verifica `jti` contra a blacklist.

**Opção B — Controller customizado:** duplicar a lógica de autenticação do EAF. Maior manutenção, frágil a upgrades.

**Recomendação:** aprovar **Opção A** e implementar em **PR-5**.

---

## 5. PR-4 — Frontends, observabilidade, DevOps, LGPD

### 5.1 Fase 5 — Frontend Game Hub

**Alterar `angular/src/app/app.routes.ts`:**

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

**Criar novos arquivos:**
- `angular/src/app/public/resolvers/game-detail.resolver.ts`
- `angular/src/app/core/guards/permission.guard.ts`
- `angular/src/app/core/constants/permissions.ts`
- `angular/src/app/public/pages/search-page/search-page.component.ts`
- `angular/src/app/public/pages/not-found/not-found.component.ts`
- `angular/src/app/public/pages/catalog-page/catalog-page.component.ts`

**Alterar `angular/src/app/player/game-frame/game-frame.component.ts`:**

```typescript
private postToGame(message: unknown): void {
  const contentWindow = this.frame?.nativeElement?.contentWindow;
  contentWindow?.postMessage(message, environment.gameOrigin);
}
```

**Alterar `angular/src/app/core/auth/token.service.ts` e `auth.service.ts`:**
- Remover `localStorage` quando PR-3.5 (HttpOnly cookie) for aprovado.
- Implementar interceptor de refresh silencioso em 401.

### 5.2 Fase 6 — Frontend Admin

**Criar `angular-admin/GameHub.UI/src/app/admin.routes.ts` conforme spec.**

**Criar:**
- `angular-admin/GameHub.UI/src/app/core/guards/auth.guard.ts`
- `angular-admin/GameHub.UI/src/app/core/guards/admin.guard.ts`
- `angular-admin/GameHub.UI/src/app/core/guards/moderator.guard.ts`
- `angular-admin/GameHub.UI/src/app/core/guards/guest.guard.ts`
- `angular-admin/GameHub.UI/src/app/resolvers/game-detail.resolver.ts`
- `angular-admin/GameHub.UI/src/app/resolvers/moderation-detail.resolver.ts`
- `angular-admin/GameHub.UI/src/app/resolvers/category-edit.resolver.ts`
- `angular-admin/GameHub.UI/src/app/resolvers/tag-edit.resolver.ts`

**Criar pages:** GameList, GameDetail, GameEdit, ReviewQueue, ReviewDetail, CategoryEdit, TagEdit.

**Ajustar:**
- `angular-admin/GameHub.UI/src/app/app-routing.module.ts`
- `angular-admin/GameHub.UI/src/app/main/main-routing.module.ts`

### 5.3 Fase 7 — DevOps

- `docker-compose.all.yml` já existe; revisar `minio-data` volume e healthcheck do MinIO.
- `install.sh`: adicionar flag `--infra` para usar `docker-compose.all.yml`.
- `Api/src/GameHub.Web.Host/appsettings*.json`: alinhar `Database.Provider` para `PostgreSQL` como padrão.
- `.env.example`: revisar `GAMEHUB_CORS_ORIGINS` e `MINIO_ENDPOINT`.

### 5.4 Fase 8 — Observabilidade e LGPD

**Criar `Api/src/GameHub.Core/Domain/Jobs/GameMetricsAggregationArgs.cs`:**

```csharp
public class GameMetricsAggregationArgs
{
    public DateTime Date { get; set; }
}
```

**Criar `Api/src/GameHub.Core/Domain/Jobs/GameMetricsAggregationJob.cs`:**

```csharp
public class GameMetricsAggregationJob : BackgroundJob<GameMetricsAggregationArgs>, ITransientDependency
{
    private readonly IRepository<GameMetricSnapshot, Guid> _snapshotRepository;
    private readonly IRepository<PlaySession, Guid> _sessionRepository;
    private readonly IRepository<GameplayEvent, Guid> _eventRepository;

    public GameMetricsAggregationJob(
        IRepository<GameMetricSnapshot, Guid> snapshotRepository,
        IRepository<PlaySession, Guid> sessionRepository,
        IRepository<GameplayEvent, Guid> eventRepository)
    {
        _snapshotRepository = snapshotRepository;
        _sessionRepository = sessionRepository;
        _eventRepository = eventRepository;
    }

    public override void Execute(GameMetricsAggregationArgs args)
    {
        var date = args.Date;
        // agrega por gameId: plays, uniquePlayers, avgDuration, loadingFinishedCount, errorCount, commercialBreakCount, rewardedBreakCount
    }
}
```

**Agendar em `Startup.cs` (dentro do bloco Hangfire):**

```csharp
RecurringJob.AddOrUpdate<GameMetricsAggregationJob>(
    "metrics-aggregation",
    j => j.Execute(new GameMetricsAggregationArgs { Date = DateTime.UtcNow.Date.AddDays(-1) }),
    Cron.Daily);
```

**Criar `Api/src/GameHub.Application/Privacy/IPrivacyAppService.cs` e `PrivacyAppService.cs`:**

```csharp
public interface IPrivacyAppService : IApplicationService
{
    Task<UserDataExportDto> ExportUserDataAsync(long userId);
    Task DeleteUserDataAsync(long userId);
}

public class PrivacyAppService : ApplicationService, IPrivacyAppService
{
    public async Task<UserDataExportDto> ExportUserDataAsync(long userId) { /* ... */ }
    public async Task DeleteUserDataAsync(long userId) { /* anonimizar/deletar soft-delete */ }
}
```

---

## 6. Testes e documentação

Para cada PR executar:

```bash
dotnet build Api/GameHub.sln -c Release --no-restore
dotnet test Api/GameHub.sln -c Release --no-build
docker compose -f docker-compose.yml config
docker compose -f docker-compose.all.yml config
```

Testes a criar/atualizar:
- `Api/test/GameHub.Tests/Builds/GameBuildPackageValidator_Tests.cs`
- `Api/test/GameHub.Tests/Builds/GameBuildAppService_Tests.cs`
- `Api/test/GameHub.Tests/Admin/AdminGameAppService_Tests.cs`
- `Api/test/GameHub.Tests/Catalog/RedisGameCatalogCache_Tests.cs`
- `Api/test/GameHub.Tests/Gameplay/RedisLeaderboardCache_Tests.cs`
- `Api/test/GameHub.Tests/Web/SecurityHeadersMiddleware_Tests.cs`
- `Api/test/GameHub.Tests/Web/ContentSecurityPolicyMiddleware_Tests.cs`
- `Api/test/GameHub.Tests/Web/CorsConfiguration_Tests.cs`
- Remover/atualizar testes de `Airplanes`.

Documentação:
- Atualizar `docs/agent-execution-log.md` após cada PR.
- Atualizar `README.md`/`README.pt-BR.md` se houver mudança de configuração.
- Manter `CHANGELOG.md`.

---

## 7. Decisões pendentes para aprovação

1. **JWT HttpOnly/Refresh Token:** aprovar abordagem (Opção A — middleware de interceptação) e se faz parte do escopo inicial ou PR separado.
2. **Extração de ZIP no MinIO:** confirmar se deseja manter o ZIP original além dos arquivos extraídos (recomendado para auditoria/reprocessamento).
3. **Permissions "own" (Developer ver/editar apenas seus jogos):** incluir no PR-2 ou separar em PR futuro.
4. **Frontend admin:** refatorar `angular-admin` para módulos do GameHub agora ou em PR-4.
5. **JWT key rotation (spec 15 5.4):** fora do escopo inicial? Recomendo deixar para PR futuro.

---

## 8. Próximos passos

Após aprovação deste plano, iniciarei pelo **PR-1** (remoção do `Airplane` + build publication), criando a branch `feature/gaps-fase-1` e seguindo a verificação e testes listados acima.
