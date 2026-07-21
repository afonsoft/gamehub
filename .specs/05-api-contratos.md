# 05 - API e Contratos

## Convenções

- Base path: `/api/services/app` quando usando Dynamic API do ABP/EAF.
- Controllers explícitos apenas para endpoints especiais como upload, download e callbacks.
- DTOs em `Application.Shared`.
- Validação por DataAnnotations e validação nativa do ABP (não FluentValidation).
- Paginação padrão ABP: `PagedAndSortedResultRequestDto`.
- Response envelope padrão ABP: `{ "result": T, "success": true, "error": null }`.
- Error codes HTTP: 400 (validação), 403 (permissão), 404 (não encontrado), 422 (regra de negócio via `UserFriendlyException`).
- API versioning via header `api-version` para evolução futura.

## Response Envelope

### Sucesso

```json
{
  "result": { ... },
  "success": true,
  "error": null
}
```

### Erro

```json
{
  "result": null,
  "success": false,
  "error": {
    "code": "GameNotFound",
    "message": "Game with id 123 was not found.",
    "details": null
  }
}
```

### Erro com detalhes de validação

```json
{
  "result": null,
  "success": false,
  "error": {
    "code": "ValidationError",
    "message": "One or more validation errors occurred.",
    "details": [
      { "field": "Title", "message": "Title is required." },
      { "field": "Slug", "message": "Slug must be lowercase and contain no spaces." }
    ]
  }
}
```

## Rate Limiting Headers

Todas as respostas incluem:

| Header | Descrição |
|--------|-----------|
| `X-RateLimit-Limit` | Limite máximo de requests no período |
| `X-RateLimit-Remaining` | Requests restantes no período |
| `X-RateLimit-Reset` | Unix timestamp de quando o período reseta |

### Limites por recurso

| Recurso | Limite | Período |
|---------|--------|---------|
| Catálogo/search | 100 req | 1 min por IP |
| Gameplay events | 30 req | 1 min por sessão |
| Leaderboard submit | 10 req | 1 min por usuário |
| Upload de build | 5 req | 1 min por dev |
| Login | 10 req | 1 min por IP |
| Reports | 5 req | 1 min por usuário |

Quando excedido, retorna `429 Too Many Requests` com body:

```json
{
  "result": null,
  "success": false,
  "error": {
    "code": "RateLimited",
    "message": "Rate limit exceeded. Try again in 45 seconds.",
    "details": null
  }
}
```

## Health Check

```http
GET /health
```

Response 200:

```json
{
  "status": "Healthy",
  "results": {
    "api": { "status": "Healthy", "duration": "12ms" },
    "postgres": { "status": "Healthy", "duration": "8ms" },
    "redis": { "status": "Healthy", "duration": "3ms" }
  }
}
```

---

## DTOs — Autenticação

### AuthenticateInput (login)

```csharp
public sealed class AuthenticateInput
{
    [Required]
    public string UserNameOrEmailAddress { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;

    public bool RememberClient { get; set; }
}
```

### AuthenticateOutput (login)

```csharp
public sealed class AuthenticateOutput
{
    public string AccessToken { get; set; } = default!;
    public string EncryptedAccessToken { get; set; } = default!;
    public int ExpireInSeconds { get; set; }
    public string RefreshToken { get; set; } = default!;
    public long? RefreshTokenExpireInSeconds { get; set; }
}
```

### RegisterInput

```csharp
public sealed class RegisterInput
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = default!;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string EmailAddress { get; set; } = default!;

    [Required]
    [StringLength(128, MinimumLength = 6)]
    public string Password { get; set; } = default!;
}
```

---

## DTOs — Catálogo (Público)

### GameCardDto

```csharp
public sealed class GameCardDto : EntityDto<Guid>
{
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string ThumbnailUrl { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string[] Categories { get; set; } = Array.Empty<string>();
    public string[] Tags { get; set; } = Array.Empty<string>();
    public bool SupportsMobile { get; set; }
    public bool SupportsDesktop { get; set; }
    public string DeveloperName { get; set; } = default!;
    public double? AverageRating { get; set; }
}
```

### GameDetailDto

```csharp
public sealed class GameDetailDto : EntityDto<Guid>
{
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public string AgeRating { get; set; } = default!;
    public string Orientation { get; set; } = default!;
    public string[] Categories { get; set; } = Array.Empty<string>();
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string ThumbnailUrl { get; set; } = default!;
    public string? HeroImageUrl { get; set; }
    public string DeveloperName { get; set; } = default!;
    public Guid? DeveloperProfileId { get; set; }
    public string PublishedBuildUrl { get; set; } = default!;
    public bool SupportsDesktop { get; set; }
    public bool SupportsMobile { get; set; }
    public bool SupportsTablet { get; set; }
    public long TotalPlays { get; set; }
    public double? AverageRating { get; set; }
    public DateTime CreationTime { get; set; }
}
```

### HomeSectionDto

```csharp
public sealed class HomeSectionDto
{
    public string SectionName { get; set; } = default!;
    public List<GameCardDto> Items { get; set; } = new();
}
```

### HomeResponseDto

```csharp
public sealed class HomeResponseDto
{
    public List<HomeSectionDto> Sections { get; set; } = new();
}
```

### SearchInput

```csharp
public sealed class SearchInput : PagedAndSortedResultRequestDto
{
    public string? Query { get; set; }
    public string[]? Categories { get; set; }
    public string[]? Tags { get; set; }
    public string? Device { get; set; }        // "desktop", "mobile", "tablet"
    public string? Orientation { get; set; }    // "Landscape", "Portrait"
}
```

### SearchResultDto

```csharp
public sealed class SearchResultDto : PagedResultDto<GameCardDto>
{
}
```

---

## DTOs — Gameplay

### StartPlaySessionInput

```csharp
public sealed class StartPlaySessionInput
{
    [Required]
    public Guid GameId { get; set; }

    [Required]
    public string DeviceType { get; set; } = default!;

    [Required]
    public string Browser { get; set; } = default!;

    public string? Referrer { get; set; }
}
```

### PlaySessionDto

```csharp
public sealed class PlaySessionDto
{
    public Guid SessionId { get; set; }
    public Guid GameId { get; set; }
    public DateTime StartedAt { get; set; }
}
```

### GameplayEventInput

```csharp
public sealed class GameplayEventInput
{
    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid GameId { get; set; }

    [Required]
    public GameplayEventType EventType { get; set; }

    public string? EventName { get; set; }

    public string? PayloadJson { get; set; }
}
```

```csharp
public enum GameplayEventType
{
    GameLoadingStarted = 0,
    GameLoadingFinished = 1,
    GameplayStarted = 2,
    GameplayStopped = 3,
    CommercialBreakRequested = 4,
    CommercialBreakCompleted = 5,
    RewardedBreakRequested = 6,
    RewardedBreakCompleted = 7,
    GameErrorCaptured = 8,
    GameMeasuredEvent = 9
}
```

---

## DTOs — Leaderboard

### SubmitScoreInput

```csharp
public sealed class SubmitScoreInput
{
    [Required]
    public Guid GameId { get; set; }

    [Required]
    public long Score { get; set; }

    public string? MetadataJson { get; set; }
}
```

### LeaderboardEntryDto

```csharp
public sealed class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = default!;
    public long Score { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### GetLeaderboardInput

```csharp
public sealed class GetLeaderboardInput
{
    [Required]
    public Guid GameId { get; set; }

    public int Take { get; set; } = 10;
}
```

---

## DTOs — Desenvolvedor

### CreateGameDraftInput

```csharp
public sealed class CreateGameDraftInput
{
    [Required]
    [StringLength(256)]
    public string Title { get; set; } = default!;

    [Required]
    [StringLength(500)]
    public string ShortDescription { get; set; } = default!;

    [StringLength(4000)]
    public string? Description { get; set; }

    [StringLength(2000)]
    public string? Instructions { get; set; }

    [Required]
    public string AgeRating { get; set; } = default!;

    [Required]
    public GameOrientation Orientation { get; set; }

    public bool SupportsDesktop { get; set; } = true;
    public bool SupportsMobile { get; set; }
    public bool SupportsTablet { get; set; }

    public Guid[]? CategoryIds { get; set; }
    public Guid[]? TagIds { get; set; }
}
```

### UpdateGameMetadataInput

```csharp
public sealed class UpdateGameMetadataInput
{
    [Required]
    public Guid GameId { get; set; }

    [Required]
    [StringLength(256)]
    public string Title { get; set; } = default!;

    [Required]
    [StringLength(500)]
    public string ShortDescription { get; set; } = default!;

    [StringLength(4000)]
    public string? Description { get; set; }

    [StringLength(2000)]
    public string? Instructions { get; set; }

    [Required]
    public string AgeRating { get; set; } = default!;

    [Required]
    public GameOrientation Orientation { get; set; }

    public bool SupportsDesktop { get; set; } = true;
    public bool SupportsMobile { get; set; }
    public bool SupportsTablet { get; set; }

    public Guid[]? CategoryIds { get; set; }
    public Guid[]? TagIds { get; set; }
}
```

### SubmitGameForReviewInput

```csharp
public sealed class SubmitGameForReviewInput
{
    [Required]
    public Guid GameId { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
```

### UploadGameBuildResultDto

```csharp
public sealed class UploadGameBuildResultDto
{
    public Guid BuildId { get; set; }
    public string Version { get; set; } = default!;
    public string Status { get; set; } = default!;
    public ValidationSummaryDto ValidationSummary { get; set; } = default!;
}

public sealed class ValidationSummaryDto
{
    public bool IsValid { get; set; }
    public string[] Errors { get; set; } = Array.Empty<string>();
    public string[] Warnings { get; set; } = Array.Empty<string>();
    public long PackageSizeBytes { get; set; }
    public string HashSha256 { get; set; } = default!;
}
```

---

## DTOs — Administração

### ApproveBuildInput

```csharp
public sealed class ApproveBuildInput
{
    [Required]
    public Guid GameBuildId { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
```

### RejectBuildInput

```csharp
public sealed class RejectBuildInput
{
    [Required]
    public Guid GameBuildId { get; set; }

    [Required]
    [StringLength(1000)]
    public string Reason { get; set; } = default!;
}
```

### PublishGameInput

```csharp
public sealed class PublishGameInput
{
    [Required]
    public Guid GameId { get; set; }
}
```

### SuspendGameInput

```csharp
public sealed class SuspendGameInput
{
    [Required]
    public Guid GameId { get; set; }

    [Required]
    [StringLength(2000)]
    public string Reason { get; set; } = default!;
}
```

### CreateOrUpdateCategoryInput

```csharp
public sealed class CreateOrUpdateCategoryInput
{
    public Guid? Id { get; set; }

    [Required]
    [StringLength(128)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(128)]
    public string Slug { get; set; } = default!;

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### CreateOrUpdateTagInput

```csharp
public sealed class CreateOrUpdateTagInput
{
    public Guid? Id { get; set; }

    [Required]
    [StringLength(64)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(64)]
    public string Slug { get; set; } = default!;
}
```

---

## DTOs — Moderação

### CompleteReviewInput

```csharp
public sealed class CompleteReviewInput
{
    [Required]
    public Guid ReviewId { get; set; }

    [Required]
    public ModerationDecision Decision { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}

public enum ModerationDecision
{
    Approved = 0,
    Rejected = 1,
    RequiresChanges = 2
}
```

### ModerationReviewDto

```csharp
public sealed class ModerationReviewDto : EntityDto<Guid>
{
    public Guid GameId { get; set; }
    public string GameTitle { get; set; } = default!;
    public Guid GameBuildId { get; set; }
    public string ReviewerName { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? Decision { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

---

## DTOs — Reportes

### UserReportDto

```csharp
public sealed class UserReportDto : EntityDto<Guid>
{
    public Guid GameId { get; set; }
    public string GameTitle { get; set; } = default!;
    public Guid? UserId { get; set; }
    public string Reason { get; set; } = default!;
    public string? Description { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
```

---

## Endpoints públicos (Game Hub)

### Autenticação

```http
POST /api/TokenAuth/Authenticate                        ← Login, retorna token JWT
POST /api/services/app/Account/Register                  ← Registro de novo usuário
GET  /api/services/app/Account/IsTenantAvailable          ← Verificar tenant
```

### Catálogo

```http
GET /api/services/app/GameCatalog/GetHome                ← Home com seções (featured, trending, new, etc.)
GET /api/services/app/GameCatalog/GetGames?skipCount=0&maxResultCount=24&sorting=popular
GET /api/services/app/GameCatalog/GetBySlug?slug=drive-example
GET /api/services/app/GameCatalog/Search?query=racing&categories=racing&categories=action&device=mobile
GET /api/services/app/GameCatalog/GetRelated?gameId={id}
```

### Gameplay

```http
POST /api/services/app/Gameplay/StartSession             ← Retorna PlaySessionDto
POST /api/services/app/Gameplay/Event                    ← Envia evento de gameplay (10 tipos)
```

### Leaderboard

```http
POST /api/services/app/Leaderboard/SubmitScore
GET  /api/services/app/Leaderboard/GetTop?gameId={id}&take=10
```

### Reports

```http
POST /api/services/app/UserReport/Submit                 ← Reportar jogo (Player autenticado ou anônimo)
```

## Endpoints de desenvolvedor (Game Hub)

```http
POST /api/services/app/DeveloperProfile/CreateOrUpdate
GET  /api/services/app/DeveloperProfile/GetMyProfile
GET  /api/services/app/DeveloperGame/GetMyGames
POST /api/services/app/DeveloperGame/CreateDraft         ← Retorna GameDetailDto
POST /api/services/app/DeveloperGame/UpdateMetadata      ← UpdateGameMetadataInput
POST /api/services/app/DeveloperGame/SubmitForReview     ← SubmitGameForReviewInput
POST /api/game-builds/{gameId}/upload                    ← UploadGameBuildInput (multipart/form-data)
GET  /api/services/app/DeveloperGame/GetBuilds?gameId={id}
```

## Endpoints administrativos (Admin)

### Gestão de Jogos

```http
GET  /api/services/app/AdminGame/GetAll                  ← SearchResultDto com filtros
GET  /api/services/app/AdminGame/GetDetail?gameId={id}   ← GameDetailDto
POST /api/services/app/AdminGame/ApproveBuild            ← ApproveBuildInput
POST /api/services/app/AdminGame/RejectBuild             ← RejectBuildInput
POST /api/services/app/AdminGame/Publish                 ← PublishGameInput
POST /api/services/app/AdminGame/Suspend                 ← SuspendGameInput
```

### Categorias e Tags

```http
POST /api/services/app/Category/CreateOrUpdate           ← CreateOrUpdateCategoryInput
GET  /api/services/app/Category/GetAll
DELETE /api/services/app/Category/Delete?id={id}
POST /api/services/app/Tag/CreateOrUpdate                ← CreateOrUpdateTagInput
GET  /api/services/app/Tag/GetAll
DELETE /api/services/app/Tag/Delete?id={id}
```

### Moderação

```http
GET  /api/services/app/Moderation/GetPendingReviews
GET  /api/services/app/Moderation/GetDetail?reviewId={id}
POST /api/services/app/Moderation/CompleteReview         ← CompleteReviewInput
```

### Reports

```http
GET  /api/services/app/AdminReport/GetAll
PUT  /api/services/app/AdminReport/UpdateStatus?reportId={id}&status={status}
```

### Dashboard

```http
GET  /api/services/app/AdminDashboard/GetSummary         ← total games, pending reviews, total plays, active users
GET  /api/services/app/AdminDashboard/GetPlaysOverTime   ← timeseries de plays
```

### Feature Flags

```http
GET  /api/services/app/FeatureFlag/GetAll
PUT  /api/services/app/FeatureFlag/Toggle?id={id}&isEnabled={bool}
```

### Audit Log

```http
GET  /api/services/app/AuditLog/GetAll?skipCount=0&maxResultCount=50&startTime=...&endTime=...
```

## Padrões de erro

- Usar `UserFriendlyException` para erro de negócio esperado (retorna 422).
- Não retornar stack trace para frontend.
- Logar exceções com CorrelationId.
- Padronizar mensagens localizáveis via `IStringLocalizer`.

## Padrões de Upload

- Aceitar `multipart/form-data`.
- Campo: `file` (IFormFile).
- Validar:
  - Tamanho máximo: 100MB.
  - Tipos permitidos: `.zip`.
  - Bloquear executáveis: `.exe`, `.dll`, `.bat`, `.cmd`, `.ps1`.
  - Validar `index.html` no pacote.
  - Gerar SHA256 do pacote.
  - Gerar `BuildVersion` (semver) incrementando automaticamente.
